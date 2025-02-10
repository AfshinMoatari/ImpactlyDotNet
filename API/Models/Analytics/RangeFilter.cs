using System;

namespace API.Models.Analytics
{

    public interface IRangeFilter
    {
        DateTime SearchStart { get; set; }
        DateTime SearchEnd { get; set; }
    }
    
    public class RangeFilter : IRangeFilter
    {
        public DateTime SearchStart { get; set; }
        public DateTime SearchEnd { get; set; }
    }
}