using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ConditionPredictor.Web.Models.CTakes;

public class CTakesResponse
{
    [JsonPropertyName("_context")]
    public Context Root { get; set; }
    [JsonPropertyName("_views")]
    public ViewsInfo Views { get; set; }
    [JsonPropertyName("_referenced_fss")]
    public Dictionary<string, object> ReferencedFss { get; set; }

    public class UmlsConcept
    {
        [JsonPropertyName("codingScheme")]
        public string CodingScheme { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonPropertyName("disambiguated")]
        public bool Disambiguated { get; set; }

        [JsonPropertyName("cui")]
        public string Cui { get; set; }

        [JsonPropertyName("tui")]
        public string Tui { get; set; }

        [JsonPropertyName("preferredText")]
        public string PreferredText { get; set; }
    }

    public class DiseaseDisorderMention
    {
        [JsonPropertyName("sofa")]
        public int Sofa { get; set; }

        [JsonPropertyName("begin")]
        public int Begin { get; set; }

        [JsonPropertyName("end")]
        public int End { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ontologyConceptArr")]
        public List<UmlsConcept> OntologyConceptArr { get; set; }

        [JsonPropertyName("typeID")]
        public int TypeID { get; set; }

        [JsonPropertyName("discoveryTechnique")]
        public int DiscoveryTechnique { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("polarity")]
        public int Polarity { get; set; }

        [JsonPropertyName("uncertainty")]
        public int Uncertainty { get; set; }

        [JsonPropertyName("conditional")]
        public bool Conditional { get; set; }

        [JsonPropertyName("generic")]
        public bool Generic { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("historyOf")]
        public int HistoryOf { get; set; }
    }

    public class Context
    {
        [JsonPropertyName("_types")]
        public Dictionary<string, TypesInfo> Types { get; set; }

        public class TypesInfo 
        {
            [JsonPropertyName("_id")]
            public string Id { get; set; }

            [JsonPropertyName("_feature_types")]
            public Dictionary<string, string> FeatureTypes { get; set; }

            [JsonPropertyName("_subtypes")]
            public List<string> Subtypes { get; set; }
        }

    }

    public class ViewsInfo
    {
        [JsonPropertyName("_InitialView")]
        public InitialViewInfo InitialView { get; set; }

        public class InitialViewInfo
        {
            [JsonPropertyName("NonEmptyFSList")]
            public List<object> NonEmptyFSList { get; set; } // Can be int or {tail,head} object

            [JsonPropertyName("DocumentAnnotation")]
            public List<DocumentAnnotation> DocumentAnnotations { get; set; }

            [JsonPropertyName("DocumentID")]
            public List<DocumentID> DocumentIDs { get; set; }

            [JsonPropertyName("DocumentIdPrefix")]
            public List<DocumentIdPrefix> DocumentIdPrefixes { get; set; }

            [JsonPropertyName("DocumentPath")]
            public List<DocumentPath> DocumentPaths { get; set; }

            [JsonPropertyName("Metadata")]
            public List<Metadata> Metadatas { get; set; }

            [JsonPropertyName("Chunk")]
            public List<Chunk> Chunks { get; set; }

            [JsonPropertyName("ConllDependencyNode")]
            public List<object> ConllDependencyNodes { get; set; } // Can be int or ConllDependencyNode

            [JsonPropertyName("NewlineToken")]
            public List<NewlineToken> NewlineTokens { get; set; }

            [JsonPropertyName("NumToken")]
            public List<NumToken> NumTokens { get; set; }

            [JsonPropertyName("PunctuationToken")]
            public List<PunctuationToken> PunctuationTokens { get; set; }

            [JsonPropertyName("SymbolToken")]
            public List<SymbolToken> SymbolTokens { get; set; }

            [JsonPropertyName("WordToken")]
            public List<WordToken> WordTokens { get; set; }

            [JsonPropertyName("Predicate")]
            public List<int> Predicates { get; set; }

            [JsonPropertyName("SemanticArgument")]
            public List<int> SemanticArguments { get; set; }

            [JsonPropertyName("SemanticRoleRelation")]
            public List<int> SemanticRoleRelations { get; set; }

            [JsonPropertyName("Segment")]
            public List<Segment> Segments { get; set; }

            [JsonPropertyName("Sentence")]
            public List<Sentence> Sentences { get; set; }

            [JsonPropertyName("MedicationMention")]
            public List<MedicationMention> MedicationMentions { get; set; }

            [JsonPropertyName("ProcedureMention")]
            public List<ProcedureMention> ProcedureMentions { get; set; }

            [JsonPropertyName("SignSymptomMention")]
            public List<SignSymptomMention> SignSymptomMentions { get; set; }

            [JsonPropertyName("AnatomicalSiteMention")]
            public List<AnatomicalSiteMention> AnatomicalSiteMentions { get; set; }

            [JsonPropertyName("DiseaseDisorderMention")]
            public List<DiseaseDisorderMention> DiseaseDisorderMentions { get; set; }
        }

        // -- Types for each annotation --
        public class DocumentAnnotation
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("language")]
            public string Language { get; set; }
        }
        public class DocumentID
        {
            [JsonPropertyName("documentID")]
            public string DocumentIDValue { get; set; }
        }
        public class DocumentIdPrefix
        {
            [JsonPropertyName("documentIdPrefix")]
            public string Prefix { get; set; }
        }
        public class DocumentPath
        {
            [JsonPropertyName("documentPath")]
            public string Path { get; set; }
        }
        public class Metadata
        {
            [JsonPropertyName("patientIdentifier")]
            public string PatientIdentifier { get; set; }
            [JsonPropertyName("patientID")]
            public int PatientID { get; set; }
            [JsonPropertyName("sourceData")]
            public SourceData SourceData { get; set; }
            [JsonPropertyName("demographics")]
            public Demographics Demographics { get; set; }
        }
        public class SourceData
        {
            [JsonPropertyName("_type")]
            public string Type { get; set; }
            [JsonPropertyName("noteTypeCode")]
            public string NoteTypeCode { get; set; }
            [JsonPropertyName("sourceInstanceId")]
            public string SourceInstanceId { get; set; }
            [JsonPropertyName("sourceRevisionNbr")]
            public int SourceRevisionNbr { get; set; }
            [JsonPropertyName("sourceRevisionDate")]
            public string SourceRevisionDate { get; set; }
        }
        public class Demographics
        {
            [JsonPropertyName("_type")]
            public string Type { get; set; }
        }
        public class Chunk
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("chunkType")]
            public string ChunkType { get; set; }
        }
        public class NewlineToken
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("tokenNumber")]
            public int TokenNumber { get; set; }
        }
        public class NumToken
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("tokenNumber")]
            public int TokenNumber { get; set; }
            [JsonPropertyName("partOfSpeech")]
            public string PartOfSpeech { get; set; }
            [JsonPropertyName("numType")]
            public int NumType { get; set; }
        }
        public class PunctuationToken
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("tokenNumber")]
            public int TokenNumber { get; set; }
            [JsonPropertyName("partOfSpeech")]
            public string PartOfSpeech { get; set; }
        }
        public class SymbolToken
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("tokenNumber")]
            public int TokenNumber { get; set; }
            [JsonPropertyName("partOfSpeech")]
            public string PartOfSpeech { get; set; }
        }
        public class WordToken
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("tokenNumber")]
            public int TokenNumber { get; set; }
            [JsonPropertyName("partOfSpeech")]
            public string PartOfSpeech { get; set; }
            [JsonPropertyName("capitalization")]
            public int Capitalization { get; set; }
            [JsonPropertyName("numPosition")]
            public int NumPosition { get; set; }
        }
        public class Segment
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("preferredText")]
            public string PreferredText { get; set; }
        }
        public class Sentence
        {
            [JsonPropertyName("sofa")]
            public int Sofa { get; set; }
            [JsonPropertyName("begin")]
            public int Begin { get; set; }
            [JsonPropertyName("end")]
            public int End { get; set; }
            [JsonPropertyName("sentenceNumber")]
            public int SentenceNumber { get; set; }
        }

        // Use your previously defined DiseaseDisorderMention and UmlsConcept for these:
        public class MedicationMention : DiseaseDisorderMention { }
        public class ProcedureMention : DiseaseDisorderMention { }
        public class SignSymptomMention : DiseaseDisorderMention { }
        public class AnatomicalSiteMention : DiseaseDisorderMention { }
    }


}