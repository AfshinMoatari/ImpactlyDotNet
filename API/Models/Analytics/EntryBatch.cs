using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Strategy;
using Nest;

namespace API.Models.Analytics
{
    [DynamoDBTable(TableNames.Analytics)]
    public class EntryBatch : CrudModel
    {
        public const string Prefix = "BATCH";

        public const string GlobalSecondaryIndex2 = "SK-GSISK-index-2";
        public const string GlobalSecondaryIndex3 = "SK-GSISK-index-3";

        [PropertyName("PK")]
        [JsonIgnore, DynamoDBHashKey]
        public override string PK { get; set; }

        [PropertyName("SK")]
        [JsonIgnore, DynamoDBRangeKey]
        public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex2)]
        public string GSIPK2 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex2)]
        public string GSISK2 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex3)]
        public string GSIPK3 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex3)]
        public string GSISK3 { get; set; }

        [PropertyName("SurveyId")] public string SurveyId { get; set; }
        [PropertyName("CodeId")] public string CodeId { get; set; }

        [PropertyName("StrategyId")] public string StrategyId { get; set; }
        [PropertyName("PatientId")] public string PatientId { get; set; }
        [PropertyName("ProjectId")] public string ProjectId { get; set; }
        [PropertyName("AnsweredAt")] public DateTime AnsweredAt { get; set; }

        [PropertyName("Score")]
        [Number(DocValues = false, IgnoreMalformed = true, Coerce = true)]
        public double Score { get; set; }
        
        [PropertyName("AverageScore")]
        [Number(DocValues = false, IgnoreMalformed = true, Coerce = true)]
        public double AverageScore { get; set; }
        

        [DynamoDBIgnore]
        [PropertyName("Entries")]
        public IEnumerable<FieldEntry> Entries { get; set; }

        [PropertyName("Tags")]
        [Keyword(Name = "Tags")]
        public List<string> Tags { get; set; }
    }
}