namespace API.Models.Analytics
{
    public class RegistrationAccess : RangeFilter
    {
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public string PatientId { get; set; }
        public string? EffectId { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }

        public string[] Tags { get; set; }
    }
}