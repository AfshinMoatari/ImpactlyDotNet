using API.Models.Analytics;

namespace API.Models.Cron
{
    public class SurveyJobAccess
    {
        public string PatientId { get; set; }
        public string StrategyId { get; set; }
    }
}