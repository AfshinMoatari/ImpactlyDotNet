using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Auth
{
    [DynamoDBTable(TableNames.Roles)]
    public class Role : CrudModel
    {
        public const string Prefix = "ROLE";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        public List<string> Permissions { get; set; }
    }
}