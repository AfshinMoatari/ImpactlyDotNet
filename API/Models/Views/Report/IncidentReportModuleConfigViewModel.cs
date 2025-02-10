using System;
using System.Collections.Generic;
using API.Models.Projects;
using API.Models.Reports;

namespace API.Models.Views.Report
{
    public class IncidentReportModuleConfigViewModel
    {
        public string EffectId { get; set; }
        public bool isEmpty { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public List<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
        public string TimeUnit { get; set; }
        public string TimePreset { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Type { get; set; }
        public bool slantedLabel { get; set; }
        public bool? labelOnInside { get; set; }

        public Layout Layout { get; set; }
    }
}