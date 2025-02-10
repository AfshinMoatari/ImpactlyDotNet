using System.Collections.Generic;
using System.Text.Json.Serialization;
using API.Models.Reports;
using Nest;
using Newtonsoft.Json;

namespace API.Models.Analytics
{
    
    public class ProjectReportResponse
    {
        [PropertyName("PopulationSize")]
        public Dictionary<string, int> PopulationSize { get; set; }
        [PropertyName("ChartDatas")]
        public List<Dictionary<string, object>> ChartDatas { get; set; }
        [PropertyName("SampleSizes")]
        public Dictionary<string, int> SampleSizes { get; set; }
        
    }

    public class ProjectReportStats
    {
        [PropertyName("FieldId")]
        public string FieldId { get; set; }
        
        [PropertyName("FieldText")]
        public string FieldText { get; set; }
        
        [PropertyName("DatePeriod")]
        public ReportModuleConfig.DateRanges DatePeriod { get; set; }
        
        [PropertyName("SmallN")]
        public int SmallN { get; set; }
        
        [PropertyName("BigN")]
        public int BigN { get; set; }

        [PropertyName("AnswerRate")]
        public decimal AnswerRate { get; set; }
        
        [PropertyName("Comments")]
        public string Comments { get; set; }

    }
}