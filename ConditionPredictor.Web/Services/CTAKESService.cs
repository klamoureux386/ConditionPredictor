using ConditionPredictor.Web.ProgramExtensions;
using ConditionPredictor.Web.Models.CTakes;
using Microsoft.AspNetCore.Components.Forms;
using System.Text;
using System.Text.Json;

namespace ConditionPredictor.Web.Services
{
    public class CTakesService
    {
        private readonly IConfiguration _configuration;

        public CTakesService(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Optionally add in a dedicated CTakesHttpClient here at some point.
        public async Task<(CTakesResponse response, string originalText)> SubmitSampleToCTAKES()
        {
            var testText = File.ReadAllText(@"C:\Users\Halifex\Documents\ConditionPredictor\ConditionPredictor.Web\CTakesTestFiles\note.txt");

            var _http = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetRequiredValue("CTakesUrl")),
                Timeout = TimeSpan.FromMinutes(2) // Set a longer timeout for cTAKES file processing
            };

            using var content = new StringContent(testText, Encoding.UTF8, "text/plain");

            var resp = await _http.PostAsync("/intake", content);
            resp.EnsureSuccessStatusCode();

            var strContent = await resp.Content.ReadAsStringAsync();

            var cTakesResponse = JsonSerializer.Deserialize<CTakesResponse>(strContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return (cTakesResponse!, testText);
        }

        public string GetCTakesInputSystemPrompt() 
        {
            return
                """
                Your objective is to revise the provided raw clinical conversation into input for Apache cTAKES, a natural language processing system for clinical text.
                Revision should reduce noise and aim to create highly explicit notes for processing.

                The below guidelines should be followed:

                1. Use Explicit Conjunctions for Lists - Always connect negated items with "and" (not "or") to avoid scope ambiguity with negation triggers like "denied."

                2. Pertinent Negation Listings - Pertinent negatives should leverage explicit repetition, such as "Denied nausea and denied vomiting" rather than "Denied nausea or vomiting."

                3. Section Your Note Deliberately - Divide your note into structured sections (e.g., HPI, PMH, Medications, Plan).NLP systems often treat content differently depending on 
                section context, so clear sectioning improves entity classification and context accuracy.

                4. Clearly Annotate Contextual Attributes - Include certainty (e.g., "possible ischemia"), temporality ("history of…"), experiencer ("family history"), conditionality, and generic status in documentation.

                5. Standardize Triggers & Phrases - Use consistent negation/scoping phrases (e.g. "no evidence of", "denies", "rule out") rather than colloquial variants to support rule-based systems like NegEx or ConText

                6. Avoid Ambiguous Constructions - Complex or run-on sentences can fragment an NLP parser’s understanding. Prefer simple sentences with one concept per clause. 
                For example, Instead of "Started aspirin 81 mg daily and lab tests pending," break it into two lines for clarity.

                7. Annotate Uncertainty & Hypotheticals - Terms like "possible," "likely," or "rule out" should be documented explicitly to convey speculative or hypothetical findings.
                """;
        }
    }
}
