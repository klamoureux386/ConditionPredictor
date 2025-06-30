using System.Text.Json.Serialization;

namespace ConditionPredictor.Web.Models.PubMed
{
    public class PMIDSearchResult
    {
        [JsonPropertyName("header")]
        public required HeaderData Header { get; set; }

        [JsonPropertyName("esearchresult")]
        public required ESearchResultData ESearchResult { get; set; }

        public class HeaderData
        {
            [JsonPropertyName("type")]
            public required string Type { get; set; }

            [JsonPropertyName("version")]
            public required string Version { get; set; }
        }

        public class ESearchResultData
        {
            [JsonPropertyName("count")]
            public required string Count { get; set; }

            [JsonPropertyName("retmax")]
            public required string RetMax { get; set; }

            [JsonPropertyName("retstart")]
            public required string RetStart { get; set; }

            [JsonPropertyName("idlist")]
            public required List<string> IdList { get; set; }

            [JsonPropertyName("translationset")]
            public required List<object> TranslationSet { get; set; }

            [JsonPropertyName("translationstack")]
            public List<object>? TranslationStack { get; set; }

            [JsonPropertyName("querytranslation")]
            public required string QueryTranslation { get; set; }
        }
    }
}
