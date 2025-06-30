namespace ConditionPredictor.ApiService.Models.PubMed
{
    public class PubmedArticleSet
    {
        public List<PubmedArticle> Articles { get; set; } = new();

        public class PubmedArticle
        {
            public MedlineCitationInfo MedlineCitation { get; set; } = new();
            public PubmedDataInfo PubmedData { get; set; } = new();

            public class MedlineCitationInfo
            {
                public string PMID { get; set; } = string.Empty;
                public DateTime? DateCompleted { get; set; }
                public DateTime? DateRevised { get; set; }
                public ArticleInfo Article { get; set; } = new();
                public MedlineJournalInfo MedlineJournalInfo { get; set; } = new();
                public List<Chemical> ChemicalList { get; set; } = new();
                public List<MeshHeading> MeshHeadingList { get; set; } = new();
                public List<string> Keywords { get; set; } = new();
                public string CoiStatement { get; set; } = string.Empty;
            }

            public class ArticleInfo
            {
                public JournalInfo Journal { get; set; } = new();
                public string ArticleTitle { get; set; } = string.Empty;
                public Pagination Pagination { get; set; } = new();
                public string DOI { get; set; } = string.Empty;
                public string AbstractText { get; set; } = string.Empty;
                public List<AuthorInfo> Authors { get; set; } = new();
                public List<string> PublicationTypes { get; set; } = new();
                public string? Language { get; set; }
                public List<DateTime?> ArticleDates { get; set; } = new();
            }

            public class JournalInfo
            {
                public string ISSN { get; set; } = string.Empty;
                public string Title { get; set; } = string.Empty;
                public string ISOAbbreviation { get; set; } = string.Empty;
                public string Volume { get; set; } = string.Empty;
                public string Issue { get; set; } = string.Empty;
                public DateTime? PubDate { get; set; }
            }

            public class Pagination
            {
                public string StartPage { get; set; } = string.Empty;
                public string MedlinePgn { get; set; } = string.Empty;
            }

            public class AuthorInfo
            {
                public string LastName { get; set; } = string.Empty;
                public string ForeName { get; set; } = string.Empty;
                public string Initials { get; set; } = string.Empty;
                public List<string> Affiliations { get; set; } = new();
            }

            public class MedlineJournalInfo
            {
                public string Country { get; set; } = string.Empty;
                public string MedlineTA { get; set; } = string.Empty;
                public string NlmUniqueID { get; set; } = string.Empty;
                public string ISSNLinking { get; set; } = string.Empty;
            }

            public class Chemical
            {
                public string RegistryNumber { get; set; } = string.Empty;
                public string NameOfSubstance { get; set; } = string.Empty;
            }

            public class MeshHeading
            {
                public string DescriptorName { get; set; } = string.Empty;
                public string DescriptorUI { get; set; } = string.Empty;
                public bool DescriptorMajorTopic { get; set; }
                public List<(string Name, string Ui, bool IsMajor)> Qualifiers { get; set; } = new();
            }

            public class PubmedDataInfo
            {
                public List<(string PubStatus, DateTime Date)> History { get; set; } = new();
                public string PublicationStatus { get; set; } = string.Empty;
                public List<(string IdType, string Value)> ArticleIds { get; set; } = new();
                public List<ReferenceInfo> References { get; set; } = new();
            }

            public class ReferenceInfo
            {
                public string Citation { get; set; } = string.Empty;
                public List<(string IdType, string Value)> ArticleIds { get; set; } = new();
            }
        }
        
    }

}
