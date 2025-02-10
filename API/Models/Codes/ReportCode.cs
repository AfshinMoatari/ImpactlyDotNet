using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Codes
{
    [DynamoDBTable(TableNames.Codes)]
    public class ReportCode : CrudModel
    {
        public const string Prefix = "REPORT";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        public string ProjectId { get; set; }
        public string ReportId { get; set; }
    }
}