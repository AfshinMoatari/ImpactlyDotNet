using API.Models.Strategy;
using System.Collections.Generic;

namespace API.Models.Views.Strategy
{
    public class BatchSendoutFrequencyViewModel
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
        public End End { get; set; }
        public string CronExpression { get; set; }
        public List<string> PatientsId { get; set; }
        public List<SurveyPropertyViewModel> Surveys { get; set; }
    }
}
