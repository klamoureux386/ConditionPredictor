using ConditionPredictor.Web.Models.PubMed;

namespace ConditionPredictor.Web.Services
{
    public class EvaluatorService
    {
        private readonly PubMedService _pubMedService;

        public EvaluatorService(PubMedService pubMedService)
        {
            _pubMedService = pubMedService;
        }

        public async Task<Dictionary<string, PubMedArticleInfo>> EvaluateCondition(string userInput) 
        {
            return await GetRelevantArticles("acute q fever[MeSH Terms]");

        }

        public async Task<Dictionary<string, PubMedArticleInfo>> GetRelevantArticles(string condition)
        {
            // Step 1: Identify relevant article PMIDs based on search terms
            //var pmids = await _pubMedService.GetRelevantPMIDs(condition);

            List<string> pmids = ["40448839", "40430780"]; 

            if (pmids.Count == 0)
            {
                Console.WriteLine("No relevant articles found for the given condition.");
                return new Dictionary<string, PubMedArticleInfo>();
            }

            // Step 2: Get article summaries from PMIDs
            return await _pubMedService.GetArticleDetailsFromPMIDs(pmids);
        }
    }
}
