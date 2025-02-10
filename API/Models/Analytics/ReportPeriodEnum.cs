using System.ComponentModel;

namespace API.Models.Analytics
{
    public class ReportPeriodEnum
    {
        public enum ReportPeriods
        {
            ThisWeek = 1,
            LastWeek = 2,
            ThisMonth = 3,
            LastMonth = 4,
            ThisQuarter = 5,
            LastQuarter = 6,
            ThisYear = 7,
            LastYear = 8,
            Adaptive = 9
        }
    }
}
