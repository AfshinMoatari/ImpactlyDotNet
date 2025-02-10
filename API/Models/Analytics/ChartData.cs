using System.Collections.Generic;

namespace API.Models.Analytics;

public class ChartData
{
    public List<Dictionary<string, object>> ChartValues { get; set; }
    public Dictionary<string, int> SampleSizes { get; set; }

    public Dictionary<string, int> PopulationSizes { get; set; }

    public ChartData(List<Dictionary<string, object>> chartValues, Dictionary<string, int> sampleSizes, Dictionary<string, int> populationSizes)
    {
        ChartValues = chartValues;
        SampleSizes = sampleSizes;
        PopulationSizes = populationSizes;
    }
    public ChartData(){
        ChartValues = new List<Dictionary<string, object>>();
        SampleSizes = new Dictionary<string, int>();
        PopulationSizes = new Dictionary<string, int>();
    }
}