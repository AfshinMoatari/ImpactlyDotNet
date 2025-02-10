using System.Collections.Generic;

namespace API.Models.Dump;

public class DumpRequestOptions
{
    public List<string> SortedBy { get; set; }
    public List<string> Filter { get; set; }
    public List<string> OrderBy { get; set; }
    public List<string> Fields { get; set; }

}