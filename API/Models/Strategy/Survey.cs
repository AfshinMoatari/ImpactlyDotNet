using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Strategy)]
    public class Survey : CrudPropModel
    {
        public const string Prefix = "SURVEY";

        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }
        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        public string Name { get; set; }
        public string LongName { get; set; }
        public string Domains { get; set; }
        public string TargetGroup { get; set; }
        public bool IsNegative { get; set; } = false;
        public string Description { get; set; }
        [DynamoDBIgnore] public IEnumerable<SurveyField> Fields { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        
        public int Index { get; set; }
        public List<int> Thresholds { get; set; }

        public string TextLanguage { get; set; } = Languages.Default;
        
        [DefaultValue(true)]
        public bool IsAverageAllowed { get; set; }

        
    }
}