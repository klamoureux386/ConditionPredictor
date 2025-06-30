using System.Xml.Linq;

namespace ConditionPredictor.Web.Utils
{
    public static class XMLUtils
    {
        public static DateTime? ParseDate(XElement? dateElem)
        {
            if (dateElem == null) return null;

            if (!int.TryParse(dateElem.Element("Year")?.Value, out var y) ||
                !int.TryParse(dateElem.Element("Month")?.Value, out var m) ||
                !int.TryParse(dateElem.Element("Day")?.Value, out var d))
                return null;

            return new DateTime(y, m, d);
        }
    }
}
