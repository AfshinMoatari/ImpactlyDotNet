namespace API.Models.Strategy
{
    public class FieldText
    {
        public const string Prefix = "TEXT";
        public int Index { get; set; }
        public string Text { get; set; }
        public string TextLanguage { get; set; }
        public int Value { get; set; }
    }
}