using System.Collections.Generic;

namespace API.Models.Views.Strategy
{
    public class StrategyViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<StrategyPatientViewModel> Patients { get; set; }
        public IEnumerable<BatchSendoutFrequencyViewModel> Frequencies { get; set; }
        public List<SurveyPropertyViewModel> Surveys { get; set; }
    }
}
