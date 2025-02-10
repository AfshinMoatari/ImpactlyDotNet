using System.Collections.Generic;
using System.Text.Json.Serialization;
using Nest;
using Newtonsoft.Json;

namespace API.Models.Analytics
{
    
    public class ReportBarChartResponse
    {
        [PropertyName("PopulationSize")]
        public int PopulationSize { get; set; }
        [PropertyName("ChartDatas")]
        public List<Dictionary<string, object>> ChartDatas { get; set; }
        [PropertyName("SampleSizes")]
        public Dictionary<string, int> SampleSizes { get; set; }
    }
}