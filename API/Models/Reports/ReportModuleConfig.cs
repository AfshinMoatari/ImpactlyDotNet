using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using API.Models.Projects;
using static API.Models.Analytics.PointSystemTypeEnum;

namespace API.Models.Reports
{
    public class ReportModuleConfig
    {
        public const string FilterExcludeOnlyOneAnswer = "ExcludeOnlyOneAnswer";
        public const string XAxisDataTypeChoices = "choices";
        public const string XAxisDataTypePeriods = "periods";
        
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

        public string EnglishName { get; set; }
        public string ProjectId { get; set; }
        
        public string StrategyId { get; set; }
        public string StrategyName { get; set; }

        public string SurveyId { get; set; }
        public string FieldId { get; set; }
        public string EffectId { get; set; }
        public string TimeUnit { get; set; } // WEEK, MONTH, QUARTER
        public string TimePreset { get; set; }
        public string Category { get; set; } // For querying status registrations
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Layout Layout { get; set; }
        public List<DateTime?> endDates { get; set; }
        public List<DateRanges> dateRanges { get; set; }
        public bool isEmpty { get; set; }
        public PointSystemType pointSystemType { get; set; }
        public bool slantedLabel { get; set; }
        public string viewType { get; set; }
        public int? graphType { get; set; } 

        public string File { get; set; }
        
        public string FreeTextId { get; set; }
        
        public string FreeTextTitle { get; set; }
        
        public string FreeTextContents { get; set; }
        public List<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
        public bool? labelOnInside { get; set; }

        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int,string> Labels { get; set; } = new Dictionary<int, string>();

        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int, string> CustomGuideLabel { get; set; } = new Dictionary<int, string>();

        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int, string> Colors { get; set; } = new Dictionary<int, string>();

        public class DateRanges
        {
            public DateTime? start { get; set; }
            public DateTime? end { get; set; }
            
            public bool isCustomGuideLabelEnabled { get; set; }
            public string Label { get; set; }
        }
        
        public List<string> Filters { get; set; }
        
        public bool IsAverageScore { get; set; }
        
        public bool IsMultipleQuestions { get; set; }
        
        public bool IsExcludeOnlyOneAnswer { get; set; }
        
        public bool ShowTimeSeriesPopulation { get; set; }

        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int, string> MultipleQuestionsIds { get; set; } = new Dictionary<int, string>();

        public string XAxisDataType { get; set; }

        public string QuestionType { get; set; }

        public int LikertScale { get; set; }
    }


    public class DictionaryConverter : IPropertyConverter
    {
        // Converts the dictionary to a DynamoDBEntry
        public DynamoDBEntry ToEntry(object value)
        {
            if (value == null)
                return null;

            var dictionary = (Dictionary<int, string>)value;
            var document = new Document();
            foreach (var kvp in dictionary)
            {
                document[kvp.Key.ToString()] = kvp.Value;
            }

            return document;
        }

        // Converts the DynamoDBEntry back to a dictionary
        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry == null || !(entry is Document document))
                return new Dictionary<int, string>();

            var dictionary = new Dictionary<int, string>();
            foreach (var kvp in document)
            {
                if (int.TryParse(kvp.Key, out int key))
                {
                    dictionary[key] = kvp.Value.AsString();
                }
            }

            return dictionary;
        }
    }


}