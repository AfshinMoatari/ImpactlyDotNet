using System.Collections.Generic;

namespace API.Models.Analytics;

public class BarChartItem
{
    public string Id { get; set; }
    public int Value { get; set; }
    public string Text { get; set; }
    public int Index { get; set; }
    
    public List<FieldEntry> FieldEntries { get; set; }
}