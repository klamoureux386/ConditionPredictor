using Aspire.Hosting;
using ConditionPredictor.AppHost.ProgramExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var ctakes = SetupCTakesJava();

//var ctakes = builder.AddProject<Projects.CTakesWrapper>("ctakeswrapper")
//    .WithExternalHttpEndpoints();

//Add the Web frontend.
builder.AddProject<Projects.ConditionPredictor_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(ctakes.GetEndpoint("http"))
    .WaitFor(ctakes)
    .WithEnvironment("CTakesUrl", ctakes.GetEndpoint("http"));

var hfToken = builder.Configuration["HuggingFace:Token"];

//Add the BioMistral Project
//https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-python

bool useCached = false; //builder.Configuration.GetValue("BioMistral:UseCachedModel", false);
//Use AWQ version to reduce VRAM necessary for local development?
bool useDareAWQ = true;
string containerCacheDir = "/root/.cache/huggingface/hub";

//string modelDir = useCached ? $"{containerCacheDir}/models--BioMistral--BioMistral-7B" : $"BioMistral/BioMistral-7B{(useDareAWQ ? "-AWQ-QGS128-W4-GEMM" : "")}";

//To do: ensure this uses the .cache folder
bool addBioMitral = false;

if (addBioMitral)
{

    var biomistralApi = builder
        .AddContainer("biomistral-vllm", image: "vllm/vllm-openai:latest")
        .WithEnvironment("HF_TOKEN", hfToken)
        //.WithBindMount(".cache/huggingface", containerCacheDir)``
        .WithArgs(
            "--model", "BioMistral/BioMistral-7B-AWQ-QGS128-W4-GEMM",
            "--host", "0.0.0.0",
            "--port", "8000",
            //Local model handicaps to account for only having 10GiB VRAM.
            "--gpu_memory_utilization", "0.7"//, //Reduce GPU consumption to 70%
                                             //"--max-model-len", "2048",
                                             //"--max-num-seqs", "1",
                                             //"--enforce-eager",
            )
        .WithContainerRuntimeArgs(
            "--gpus=all",
            "-p", "8000:8000",
            "--ipc=host")
        .WithHttpEndpoint(port: 8001, targetPort: 8000, name: "inference");

}


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

builder.Build().Run();

IResourceBuilder<JavaAppExecutableResource> SetupCTakesJava() 
{
    //link to folder containing OpenTelemetry Java agent - opentelemetry-javaagent.jar
    var agentJarFolder = Path.GetFullPath(Path.Combine("..", "ConditionPredictor.AppHost"));

    //Path to the cTAKES folder containing the Maven build file
    var workingDir = Path.GetFullPath("..\\cTakesJava\\wrapper-app\\complete");

    //Ensure project is built/up-to-date before adding the Spring App.
    BuildCTAKESJavaMaven(workingDir);

    return builder.AddSpringApp(
        "ctakes-api",
        workingDirectory: workingDir,
        new JavaAppExecutableResourceOptions
        {
            ApplicationName = "target/spring-boot-complete-0.0.1-SNAPSHOT.jar",
            OtelAgentPath = agentJarFolder
        })
        .WithMavenBuild()
        .PublishAsDockerFile(c =>
        {
            c.WithBuildArg("JAR_NAME", "spring-boot-complete-0.0.1-SNAPSHOT.jar")
             .WithBuildArg("AGENT_PATH", "/agents")
             .WithBuildArg("SERVER_PORT", "8085");
        });
}

/// <summary>Builds the cTAKES Java Maven project.</summary>
void BuildCTAKESJavaMaven(string workingDir) 
{
    // Build cTAKES Java JAR before adding SpringApp
    var mvnCmd = Path.Combine(workingDir, "mvnw.cmd"); // use "mvnw" on Linux/Mac

    if (File.Exists(mvnCmd))
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = mvnCmd,
            Arguments = "clean package",
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