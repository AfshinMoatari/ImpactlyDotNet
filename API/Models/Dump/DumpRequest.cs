using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Dump;

public class DumpRequest
{
    public string SortedBy { get; set; }
    public string Filter { get; set; }
    
    public string OrderBy { get; set; }

    public List<string> Fields { get; set; }

    public string UserName { get; set; }
    public string ProjectName { get; set; }

    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public string AllFieldsToString()
    {
        return Fields == null ? "" : Fields.Aggregate("", (current, field) => current + (field + ", "));
    }
}