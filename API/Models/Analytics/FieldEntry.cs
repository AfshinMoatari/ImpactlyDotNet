using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using Nest;

namespace API.Models.Analytics
{
    [DynamoDBTable(TableNames.Analytics)]
    public class FieldEntry : CrudPropModel
    {
        public const string Prefix = "ENTRY";
        public const string GlobalSecondaryIndex3 = "SK-GSISK-index-3";

        [PropertyName("ProjectId")] public string ProjectId { get; set; }
        [PropertyName("SurveyId")] public string SurveyId { get; set; }
        [PropertyName("StrategyId")] public string StrategyId { get; set; }
        [PropertyName("PatientId")] public string PatientId { get; set; }
        [PropertyName("Text")] public string Text { get; set; }
        [PropertyName("Index")] public int Index { get; set; }
        [PropertyName("Value")] public double Value { get; set; }
        [PropertyName("FieldId")] public string FieldId { get; set; }
        [PropertyName("FieldText")] public string FieldText { get; set; }
        
        [PropertyName("FieldTextEnglish")] public string FieldTextEnglish { get; set; }
        [PropertyName("ChoiceId")] public string ChoiceId { get; set; }
        [PropertyName("ChoiceText")] public string ChoiceText { get; set; }
        
        [PropertyName("ChoiceTextEnglish")] public string ChoiceTextEnglish { get; set; }
        [PropertyName("AnsweredAt")] public DateTime AnsweredAt { get; set; }
        
        [PropertyName("FieldIndex")] public int FieldIndex { get; set; }
        
        
        [PropertyName("Tags")]
        [Keyword(Name = "Tags")]
        public List<string> Tags { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex3)]
        public string GSISK3 { get; set; }
        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex3)]
        public string GSIPK3 { get; set; }
    }
}