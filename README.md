This is a public-facing copy of an AI-first Clincial Decision Support System (CDSS) I'm working on.

It is a distributed application orchestrated via .Net Aspire. Individual services include:
- A Java SpringBoot app which wraps Apache [CTAKES](https://github.com/apache/ctakes), an NLP platform for extracting information from clinical text.
- A vLLM-hosted instance of a bio-focused LLM for CTAKES annotation evaluation, wrapped by a small Python+Flask app.
- A Blazor Server web application which is the user entrypoint for interacting with these services.

Both non-.Net apps are set up with OpenTelemetry to integrate with .Net Aspire's robust observability features.

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

TO DO: Project setup documentation
