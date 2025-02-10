using System;
using System.Collections.Generic;
using API.Models.Projects;
using API.Models.Reports;
using static API.Models.Analytics.PointSystemTypeEnum;

namespace API.Models.Views.Report
{
    public class StatusReportModuleConfigViewModel
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public List<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
        public int? graphType { get; set; }
        public PointSystemType pointSystemType { get; set; }
        public Layout Layout { get; set; }
        public string Type { get; set; }
        public bool isEmpty { get; set; }
        public List<DateTime?> endDates { get; set; }
        public bool slantedLabel { get; set; }
        public bool? labelOnInside { get; set; }
    }
}