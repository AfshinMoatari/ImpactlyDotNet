namespace API.Models.Analytics
{
    public class SurveyAccess : RangeFilter
    {
        public string ProjectId { get; set; }
        public string SurveyId { get; set; }
        public string PatientId { get; set; }
        public string StrategyId { get; set; }

        public string[] Tags { get; set; }
    }
}