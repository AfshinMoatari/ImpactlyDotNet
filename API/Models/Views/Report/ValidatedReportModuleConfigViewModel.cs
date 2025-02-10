using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Models.Projects;
using API.Models.Reports;
using static API.Models.Reports.ReportModuleConfig;

namespace API.Models.Views.Report
{
    public class ValidatedReportModuleConfigViewModel
    {
        public string Name { get; set; }
        public Layout Layout { get; set; }
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public string SurveyId { get; set; }
        public List<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
        public int graphType { get; set; }
        public string Type { get; set; }
        public List<DateRanges> dateRanges { get; set; }
        public bool slantedLabel { get; set; }
        public bool? labelOnInside { get; set; }
        
        public List<string> Filters { get; set; }
        
        
        public bool IsAverageScore { get; set; }
        
        public bool IsCustomLabel { get; set; }
        
        public bool CustomLabel { get; set; }
        
        public bool IsExcludeOnlyOneAnswer { get; set; }
        
        public bool ShowTimeSeriesPopulation { get; set; }
        
        public bool IsMultipleQuestions { get; set; }

        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int, string> MultipleQuestionsIds { get; set; } = new Dictionary<int, string>();
    }
}