using Qdrant.Client;

namespace ConditionPredictor.Web.Services
{
    public class VectorService
    {
        private readonly QdrantClient _qdrant;

        public VectorService(QdrantClient qdrant) 
        {
            _qdrant = qdrant;
        }

        public async Task<string> GetCollections() 
        {
            return string.Join(",", await _qdrant.ListCollectionsAsync());
        }
    }
}
