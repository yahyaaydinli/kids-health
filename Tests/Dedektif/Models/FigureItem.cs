namespace PsikologTestApp.Tests.Dedektif.Models
{
    public class FigureItem
    {
        public string ColorName { get; set; } = "";
        public string ColorHex  { get; set; } = "#000000";
        public string ShapeName { get; set; } = "";  // "kare", "A", vs.
        public string Symbol    { get; set; } = "";  // "■", "A", vs.

        public string DisplayName => $"{ColorName} {ShapeName}";
    }
}
