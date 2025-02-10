using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using System.Text.Json.Serialization;
using API.Constants;
using API.Models.Strategy;
using Nest;
using Amazon.DynamoDBv2.Model;

namespace API.Models.Analytics
{
    [DynamoDBTable(TableNames.Analytics)]
    public class Registration : CrudModel
    {
        public const string Prefix = "REGISTRATION";
        public const string GlobalSecondaryIndex2 = "SK-GSISK-index-2";
        public const string GlobalSecondaryIndex3 = "SK-GSISK-index-3";

        [PropertyName("PK")]
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [PropertyName("SK")]
        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex2)]
        public string GSIPK2 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex2)]
        public string GSISK2 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex3)]
        public string GSIPK3 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex3)]
        public string GSISK3 { get; set; }

        [PropertyName("ProjectId")] public string ProjectId { get; set; }
        [PropertyName("PatientId")] public string PatientId { get; set; }
        [PropertyName("EffectId")] public string EffectId { get; set; }
        [PropertyName("EffectName")] public string EffectName { get; set; }
        [PropertyName("Note")] public string Note { get; set; }
        [PropertyName("Date")] public DateTime Date { get; set; }
        [PropertyName("Type")] public string Type { get; set; }
        [PropertyName("Value")] public double Value { get; set; }
        
        [PropertyName("Before")]
        public StrategyEffect Before { get; set; }
        [PropertyName("Now")] 
        public StrategyEffect Now { get; set; }
        [PropertyName("Category")] public string Category { get; set; }

        [PropertyName("Tags")]
        [Keyword(Name = "Tags")]
        public List<string> Tags { get; set; }
        
        [PropertyName("DeletedAt")]
        public DateTime? DeletedAt { get; set; } = null;

        public static explicit operator Registration(StrategyEffect effect) => new Registration()
        {
            Category = effect.Category,
            Type = effect.Type,
            EffectName = effect.Name,
            GSIPK = effect.GSIPK,
            GSISK = effect.GSISK
        };
    }
}