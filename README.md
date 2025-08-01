# Introduction

This is a public-facing copy of an AI-first Clincial Decision Support System (CDSS) I'm working on.

It is a distributed application orchestrated via .NET Aspire. Individual services include:
- A Java SpringBoot app which wraps Apache [CTAKES](https://github.com/apache/ctakes), an NLP platform for extracting information from clinical text.
- A vLLM-hosted instance of a bio-focused LLM for CTAKES annotation evaluation, wrapped by a small Python+Flask app.
- A Blazor Server web application which is the user entrypoint for interacting with these services, styled with Tailwind.

Both non-.NET apps are set up with OpenTelemetry to integrate with .NET Aspire's robust observability features.

The workflow pipeline is as follows:

❌: Not Started
🔄: In progress
✅: Functional

1. ❌Speech-to-text capture of physician/patient dialogue.
2. 🔄Conversion of raw notes to optimized clinical text for initial CTAKES evaluation.
3. ❌Evaluation and correction of CTAKES output via LLM (See image 1 for example).
4. ✅Human-readable representation of identified attributes in CTAKES annotated content (See image 1 for example).
5. ✅Retrieval of relevant articles from PubMed... 🔄via MeSH terms abstracted from annotated content.
6. ❌Indexing of identfied articles into Qdrant database for RAG based on annotation. This helps build up a knowledge base over time and also avoid reprocessing articles which were already used.
7. ❌Combining all of the above to create a final **suggested** diagnosis. Ideally, this will function as an assistive tool around the diagnosis as well by providing potential treatment based on pharmacological action, a diagnosis confidence score, referenced articles, etc.

<img width="2494" height="988" alt="image" src="https://github.com/user-attachments/assets/53955150-2a4a-4cda-8dee-2705b1248b9f" />

*Image 1 - The default CTAKES annotation output styling. Note the inconsistency in the annotation for "denied nausea and vomitting."*

# Project Setup

## Aspire Setup

1. Download the [Aspire 9.4 CLI](https://learn.microsoft.com/en-us/dotnet/aspire/whats-new/dotnet-aspire-9.4) and launch the project from the AppHost directory using `aspire run`. NOTE: Running from Visual Studio results an [issue](https://github.com/dotnet/aspire/issues/10377) where the Java process (and by extension, Tomcat server occupying the port) isn't gracefully terminated on project shutdown. Shutting down via the IDE doesn't send the SIGTERM signal properly compared to the CLI.

## Java & CTakes Setup

1. Download the [latest](https://www.java.com/en/download/) version of Java (Java 17+ is required to run CTakes). This should automatically set up your JAVA_HOME path.
2. In order to use the [OpenTelemetry Agent for Java](https://opentelemetry.io/docs/zero-code/java/agent/) with .NET Aspire's OpenTelemetry, you will need to [import the .NET Aspire OpenTelemetry certificate](https://learn.microsoft.com/en-us/dotnet/aspire/community-toolkit/hosting-java?tabs=bash%2Cdotnet-cli%2Cexecutable-hosting#linux-and-macos-certificate-trust) into the Java certificate store. To do this, perform the following steps:
    1. From the .NET Aspire **dashboard**, click the lock icon next to your localhost address.
    2. Click "Connection is secure" or similar → then click "Certificate is valid".
    3. In the Certificate window:
        - Go to the Details tab.
        - Click Copy to File...
        - Use the Certificate Export Wizard at the bottom right.
        - Choose Base-64 encoded ASCII, single certificate (\*.pem;\*.crt).
        - Rename the file to aspire-dashboard.crt
    4. From an Admin Command Prompt, import your certificate into the Java truststore using the `keytool` command:
        ```
        keytool -import -trustcacerts -alias aspire-dashboard ^
            -file aspire-dashboard.crt ^
            -keystore "%JAVA_HOME%/lib/security/cacerts" ^
            -storepass [YOUR_PASSWORD_HERE]
        ```
    5. If you see: `PKIX path building failed: sun.security.provider.certpath.SunCertPathBuilderException: unable to find valid certification path to requested target` in the .NET Aspire Java logs at all, that means something went wrong during the above!
3. Download the latest version of Maven.
4. Navigate to the CTakesJava directory.
5. Clone the Apache cTAKES [fork] (https://github.com/klamoureux386/cdss-ctakes) into the CTakesJava folder of your machine. NOTE: Any cTAKES customization should be done against this repository.
6. Run the setup for cTAKES (currently just `mvn clean compile` and `mvn clean compile package`).
7. Afterward, take the compiled ctakes-distribution target folder (*cTakesJava/ctakes/ctakes-distribution/target*) and move the `apache-ctakes-#.#.#.-bin.zip` file outward to the same directory as the parent ctakes folder.
8. Unzip the bin .zip, rename the output folder to `cdss-ctakes-7.0.0-SNAPSHOT`
9. Make sure you [download](https://sourceforge.net/projects/ctakesresources/files/sno_rx_16ab.zip/download) the default fast lookup dictionary and unzip it in your `cdss-ctakes-7.0.0-SNAPSHOT/resources/org/apache/ctakes/dictionary/lookup/fast` directory.

10. To run the CTakes Java wrapper standalone if testing:
    1. Open up the wrapper-app project in IntelliJ.
    2. Set up a Configuration to run the app. Edit Configurations → Add New Application → Select *CDSS Application* for the Main Class → Apply. Run the application.
    3. You may need to right click the project and hit Maven → Sync Project if you get any errors about dependencies.

## Python & vLLM Setup