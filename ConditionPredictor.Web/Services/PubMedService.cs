using ConditionPredictor.Web.Models.PubMed;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace ConditionPredictor.Web.Services
{
    public class PubMedService
    {
        private readonly HttpClient _httpClient;

        public PubMedService(HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetRelevantPMIDs(string searchTerm) 
        {
            using var client = new HttpClient();

            var retMax = 2;

            var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esearch.fcgi?db=pubmed&term={Uri.EscapeDataString(searchTerm)}&retmode=json&retmax={retMax}";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<PMIDSearchResult>(json, options);

            if (result?.ESearchResult?.IdList != null)
            {
                foreach (var id in result.ESearchResult.IdList)
                {
                    Console.WriteLine($"PMID: {id}");
                }
            }

            return result?.ESearchResult?.IdList ?? [];
        }

        public async Task<Dictionary<string, PubMedArticleInfo>> GetArticleDetailsFromPMIDs(List<string> pmIds)
        {
            var ids = string.Join(",", pmIds);
            var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/esummary.fcgi" +
                      $"?db=pubmed&id={ids}&retmode=json";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var summary = JsonSerializer.Deserialize<PubMedSummaryResponse>(json, options);

            return summary!.Result.GetArticles();
        }

        public async Task<PubmedArticleSet?> GetPubmedArticle(string pmid)
        {
            var url = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi" +
              $"?db=pubmed&id={pmid}&retmode=xml";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            using var xmlStream = await response.Content.ReadAsStreamAsync();
            var doc = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);

            var root = doc.Root; // <PubmedArticleSet>
            var firstArticleElement = root?.Element("PubmedArticle");
            if (firstArticleElement == null)
                return new PubmedArticleSet();

            var medline = firstArticleElement.Element("MedlineCitation");
            var articleElem = medline?.Element("Article");

            string? doi = articleElem?
                .Elements("ELocationID")
                .FirstOrDefault(el => (string?)el.Attribute("EIdType") == "doi")?.Value ?? "";

            var journalElement = articleElem?.Element("Journal");
            var journal = new PubmedArticleSet.PubmedArticle.JournalInfo
            {
                ISSN = journalElement?.Element("ISSN")?.Value ?? "",
                Title = journalElement?.Element("Title")?.Value ?? "",
                ISOAbbreviation = journalElement?.Element("ISOAbbreviation")?.Value ?? "",
                Volume = journalElement?.Element("JournalIssue")?.Element("Volume")?.Value ?? "",
                Issue = journalElement?.Element("JournalIssue")?.Element("Issue")?.Value ?? "",
                PubDate = ParseDate(journalElement?.Element("JournalIssue")?.Element("PubDate"))
            };

            var language = articleElem?.Element("Language")?.Value ?? "";

            var publicationTypes = articleElem?.Element("PublicationTypeList")?.Elements("PublicationType").Select(pt => pt.Value).ToList() ?? new();

            var articleDates = articleElem?.Elements("ArticleDate")
                .Select(ParseDate).ToList() ?? new();

            var pubmedData = firstArticleElement.Element("PubmedData");
            var history = pubmedData?
                .Element("History")?
                .Elements("PubMedPubDate")
                .Select(e => (
                    PubStatus: (string?)e.Attribute("PubStatus") ?? "",
                    Date: new DateTime(
                        int.Parse(e.Element("Year")?.Value ?? "0"),
                        int.Parse(e.Element("Month")?.Value ?? "1"),
                        int.Parse(e.Element("Day")?.Value ?? "1")
                    )
                ))
                .ToList() ?? new();

            var authors = articleElem?
                .Element("AuthorList")?
                .Elements("Author")
                .Select(a => new PubmedArticleSet.PubmedArticle.AuthorInfo()
                {
                    LastName = a.Element("LastName")?.Value ?? "",
                    ForeName = a.Element("ForeName")?.Value ?? "",
                    Initials = a.Element("Initials")?.Value ?? "",
                    Affiliations = a.Elements("AffiliationInfo")
                                    .Select(ai => ai.Element("Affiliation")?.Value)
                                    .Where(s => s != null)
                                    .ToList()!
                }).ToList() ?? new();

            var abstractText = string.Join("\n\n",
                articleElem?
                    .Element("Abstract")?
                    .Elements("AbstractText")
                    .Select(el => (string)el)
                    .Where(s => !string.IsNullOrWhiteSpace(s)) ?? Enumerable.Empty<string>()
            );

            return new PubmedArticleSet
            {
                Articles = new List<PubmedArticleSet.PubmedArticle>
                {
                    new PubmedArticleSet.PubmedArticle
                    {
                        MedlineCitation = new PubmedArticleSet.PubmedArticle.MedlineCitationInfo()
                        {
                            PMID = medline!.Element("PMID")!.Value,
                            DateCompleted = ParseDate(medline?.Element("DateCompleted")),
                            DateRevised = ParseDate(medline?.Element("DateRevised")),
                            Article = new PubmedArticleSet.PubmedArticle.ArticleInfo
                            {
                                Journal = journal,
                                ArticleTitle = articleElem?.Element("ArticleTitle")?.Value ?? "",
                                DOI = doi,
                                AbstractText = abstractText,
                                Authors = authors,
                                Language = articleElem?.Element("Language")?.Value ?? "",
                                PublicationTypes = publicationTypes,
                                ArticleDates = articleDates,
                                // You can parse additional fields as needed...
                            }
                        },
                        PubmedData = new PubmedArticleSet.PubmedArticle.PubmedDataInfo { History = history }
                    }
                }
            };

        }
    }
}
