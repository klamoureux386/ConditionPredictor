package com.example.springboot;

import org.springframework.web.bind.annotation.*;
import org.springframework.http.ResponseEntity;
import org.springframework.http.MediaType;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.time.Instant;

@RestController
public class IntakeController {

    private static final Logger logger = LoggerFactory.getLogger(IntakeController.class);

    @PostMapping(value = "/intake", consumes = MediaType.TEXT_PLAIN_VALUE)
    public ResponseEntity<?> index(@RequestBody String text) {
        String currentDir = System.getProperty("user.dir");
        Path cTakesPath = Paths.get(currentDir).getParent().getParent().resolve("ctakes-7.0.0-compiled");
        Path cTakesBin = cTakesPath.resolve("bin");
        Path batFile = cTakesBin.resolve("runClinicalPipeline.bat");
        Path piperFile = cTakesPath.resolve("resources/org/apache/ctakes/clinical/pipeline/CustomFastPipeline.piper");

        String inputPath = "C:\\ctakes-test\\input";
        String outputPath = "C:\\ctakes-test\\output";
        String key = "30254dd6-f1b5-4cea-a2f5-b194105a8fc7";

        // Create file and run pipeline
        String inputFileName = "input_" + Instant.now().toEpochMilli() + ".txt";
        Path inputFile = Paths.get(inputPath, inputFileName);
        String outputFileName = inputFileName + ".pretty.html";
        String outputXmiName = inputFileName + ".xmi";
        Path outputFile = Paths.get(outputPath, outputFileName);
        Path outputXmi = Paths.get(outputPath, outputXmiName);

        try {
            // Validate files
            if (!Files.exists(batFile)) throw new IOException("Cannot find: " + batFile);
            if (!Files.exists(piperFile)) throw new IOException("Cannot find: " + piperFile);

            // Ensure input directory exists
            Files.createDirectories(Paths.get(inputPath));

            // Write input
            Files.write(inputFile, text.getBytes(StandardCharsets.UTF_8));

            // Build the process
            ProcessBuilder pb = new ProcessBuilder(
                    "cmd.exe", "/c", batFile.toString(),
                    "-i", inputPath,
                    "-o", outputPath,
                    "--xmiOut", outputPath,
                    "--piper", piperFile.toString(),
                    "--key", key
            );
            pb.directory(cTakesBin.toFile());
            pb.redirectErrorStream(true);

            // Start process and collect logs
            Process process = pb.start();
            StringBuilder logs = new StringBuilder();
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(process.getInputStream(), StandardCharsets.UTF_8))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    logs.append(line).append(System.lineSeparator());
                }
            }
            int exitCode = process.waitFor();
            logger.info("Process exited with code: {}. Output: {}", exitCode, logs);

            // Poll for output file
            int maxTries = 5;
            int currentTry = 1;
            while (!Files.exists(outputFile)) {
                if (currentTry++ > maxTries) throw new RuntimeException("Exceeded max retries while polling for cTAKES output.");
                Thread.sleep(5000);
            }

            // Read output
            String html = Files.readString(outputFile, StandardCharsets.UTF_8);

            // Delete input file
            Files.deleteIfExists(inputFile);

            //TO DO
            var json = XmiUtils.XmiToJson(outputXmi.toString());

            return ResponseEntity.ok()
                    .contentType(MediaType.APPLICATION_JSON)
                    .body(json);

        } catch (Exception ex) {
            logger.error("Error running cTAKES pipeline", ex);
            return ResponseEntity.status(500).body("Error running batch: " + ex.getMessage());
        }
    }



}
