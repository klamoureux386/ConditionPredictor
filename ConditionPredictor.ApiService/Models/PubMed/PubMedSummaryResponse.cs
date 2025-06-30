using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConditionPredictor.ApiService.Models.PubMed
{
    public class PubMedSummaryResponse
    {
        [JsonPropertyName("header")]
        public required HeaderData Header { get; set; }

        [JsonPropertyName("result")]
        public required ResultData Result { get; set; }

        public class HeaderData
        {
            [JsonPropertyName("type")]
            public required string Type { get; set; }

            [JsonPropertyName("version")]
            public required string Version { get; set; }
        }

        public class ResultData
        {
            [JsonPropertyName("uids")]
            public required List<string> Uids { get; set; }

            //Deserialize to PubMedArticleSummary
            [JsonExtensionData]
            public Dictionary<string, JsonElement> Articles { get; set; } = new();

            public Dictionary<string, PubMedArticleInfo> GetArticles()
            {
                var articles = new Dictionary<string, PubMedArticleInfo>();
                foreach (var uid in Uids)
                {
                    if (Articles.TryGetValue(uid, out var articleElement))
                    {
                        var article = JsonSerializer.Deserialize<PubMedArticleInfo>(articleElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (article != null)
                        {
                            articles[uid] = article;
                        }
                    }
                }
                return articles;
            }
        }
    }

    public class PubMedArticleInfo
    {
        [JsonPropertyName("uid")]
        public required string Uid { get; set; }

        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("source")]
        public required string Source { get; set; }

        [JsonPropertyName("pubdate")]
        public required string PubDate { get; set; }

        [JsonPropertyName("authors")]
        public required List<Author> Authors { get; set; }

        public class Author
        {
            [JsonPropertyName("name")]
            public required string Name { get; set; }
        }
    }

}
