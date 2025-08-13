using Aspire.Hosting;
using ConditionPredictor.AppHost.ProgramExtensions;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var ctakes = SetupCTakesJava();

#pragma warning disable ASPIREHOSTINGPYTHON001
var pythonApp = builder.AddPythonApp("BioMistralFastAPI", "../BioMistralFastAPI", "main.py")
       .WithHttpEndpoint(env: "PORT")
       .WithExternalHttpEndpoints()
       .WithOtlpExporter();
#pragma warning restore ASPIREHOSTINGPYTHON001

if (builder.ExecutionContext.IsRunMode && builder.Environment.IsDevelopment())
{
    pythonApp.WithEnvironment("DEBUG", "True");
}

var vLLM = SetupMediPhivLLM();

var qdrant = SetupQdrant();

//Add the Web frontend.
builder.AddProject<Projects.ConditionPredictor_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(ctakes.GetEndpoint("http"))
    .WaitFor(ctakes)
    .WithEnvironment("CTakesUrl", ctakes.GetEndpoint("http"))
    .WaitFor(pythonApp)
    .WithEnvironment("Qdrant-ApiKey", qdrant.ApiKey)
    //.WaitFor(qdrant.Server)   
    .WaitFor(vLLM);

var app = builder.Build();

app.Run();

(IResourceBuilder<QdrantServerResource> Server, IResourceBuilder<ParameterResource> ApiKey) SetupQdrant()
{

    // Store the API key as a secret parameter; Aspire passes it to the container.
    var apiKey = builder.AddParameter("Qdrant-ApiKey", secret: true);

    var qdrant = builder
        //TO DO: Validate that we can pass RUN_MODE here
        .AddQdrant("qdrant", apiKey)                    // uses qdrant/qdrant image
        //.WithLifetime(ContainerLifetime.Persistent)   // Qdrant is slow to warm. Disabled until finished development.
        .WithDataVolume()                               // mounts a Docker volume at /qdrant/storage
        // Optional: override config (e.g., max shards, telemetry, GPU flags)
        .WithBindMount(Path.GetFullPath("infra/qdrant/config"), "/qdrant/config/");

    return (qdrant, apiKey);
}

IResourceBuilder<ContainerResource> SetupMediPhivLLM() 
{
    var hfToken = builder.Configuration["HuggingFace:Token"];

    //TO DO: Options pattern & type safety.
    string modelName = builder.Configuration["Models:MediPhi:ModelName"]!;
    string snapshotId = builder.Configuration["Models:MediPhi:SnapshotId"]!;
    bool useCached = bool.Parse(builder.Configuration["Models:MediPhi:UseCached"]!);
    string containerCacheDir = "/root/.cache/huggingface/hub";
    string modelDir = useCached ? $"{containerCacheDir}/models--{modelName.Replace("/", "--")}/snapshots/{snapshotId}" : modelName;
    string hostAdapterDir = "../LoRA-Trainer/fake-disease-adapter";
    string containerAdapterDir = "/root/LoRA-modules/fake-disease-adapter";

    List<string> vLLM_args = [
            "--model", modelDir,
            "--served-model-name", modelName,
            "--host", "0.0.0.0",
            "--port", "8000",
            //Local model handicaps to account for only having 10GiB VRAM.
            "--gpu_memory_utilization", "0.85", // Cap GPU consumption at 85%
            "--max_model_len", "1024"           // Cap max token len due to GPU limitations
        ];

    List<string> LoRA_args = [
            "--enable-lora",                    // Enable LoRA adapter support
            "--lora-modules",                   // Preload up to N adapters
                $"fake-syndrome={containerAdapterDir}",
            "--max-loras", "5"                  // Limit concurrent acitve adapters to reduce VRAM
        ];

    bool enableLoRA = bool.Parse(builder.Configuration["Models:MediPhi:EnableLoRA"]!);

    if (enableLoRA)
        vLLM_args.AddRange(LoRA_args);

    var vLLM = builder.AddContainer("mediphi-vllm", image: "vllm/vllm-openai:latest")
        .WithEnvironment("HF_TOKEN", hfToken)
        // Local cache for downloaded HF model
        .WithBindMount(".cache/huggingface/hub", containerCacheDir, isReadOnly: false)
        // Set LoRA training directory
        .WithBindMount(hostAdapterDir, containerAdapterDir, isReadOnly: false)
        .WithArgs(
            vLLM_args.ToArray()
            )
        .WithContainerRuntimeArgs(
            "--gpus=all",
            "-p", "8000:8000",
            "--ipc=host")
        .WithHttpEndpoint(port: 8001, targetPort: 8000, name: "inference");

    return vLLM;
}

IResourceBuilder<JavaAppExecutableResource> SetupCTakesJava() 
{
    //link to folder containing OpenTelemetry Java agent - opentelemetry-javaagent.jar
    var agentJarFolder = Path.GetFullPath(Path.Combine("..", "ConditionPredictor.AppHost", "agents"));
    var wrapperWorkingDir = Path.GetFullPath("..\\cTakesJava\\wrapper-app");
    var jarName = "cdss-0.0.1-SNAPSHOT.jar";

    var ctakes = builder.AddSpringApp(
        "ctakes-api",
        workingDirectory: wrapperWorkingDir,
        new JavaAppExecutableResourceOptions
        {
            ApplicationName = $"target/{jarName}",
            OtelAgentPath = agentJarFolder
        })
        .WithMavenBuild(new MavenOptions() { Command="mvnw", Args = ["compile", "package"] })
        .PublishAsDockerFile(c =>
        {
            c.WithBuildArg("JAR_NAME", jarName)
             .WithBuildArg("AGENT_PATH", "/agents")
             .WithBuildArg("SERVER_PORT", "8085");
        });

    return ctakes;
}

//EDIT: This is old/come back and figure out a way of keeping cTakes up to date easier when dev'ing custom pipelines.
//Need to also copy over/unpack the distribution target .zip
/// <summary>Builds the cTAKES Java Maven project.</summary>
void BuildCTAKESJavaMaven(IDistributedApplicationBuilder builder, string workingDir) 
{
    // Build cTAKES Java JAR before adding SpringApp
    var mvnCmd = Path.Combine(workingDir, "mvnw.cmd"); // use "mvnw" on Linux/Mac

    if (File.Exists(mvnCmd))
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = mvnCmd,
            Arguments = "clean package -e",
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        if (process == null)
            throw new Exception("Failed to start Maven process.");

        process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (s, e) => Console.Error.WriteLine(e.Data);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new Exception("Maven build failed");
    }
    else
    {
        throw new FileNotFoundException($"Could not find {mvnCmd}");
    }

}