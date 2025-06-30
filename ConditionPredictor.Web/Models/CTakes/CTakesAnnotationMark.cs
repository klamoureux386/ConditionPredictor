namespace ConditionPredictor.Web.Models.CTakes
{
    public class CTakesAnnotationMark
    {
        public int Begin { get; set; }
        public int End { get; set; }
        public string Type { get; set; }
        public string PreferredText { get; set; }
        public string Tooltip { get; set; }
        public string CssClass { get; set; }
        // ...other fields (like CUI, TUI, etc.)
    }
}
