using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Reports;

namespace API.Models.Projects
{
    [DynamoDBTable(TableNames.Projects)]
    public class Project : CrudModel
    {
        public const string Prefix = "PROJECT";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        [Required] public string Name { get; set; }

        public string SroiUrl { get; set; }
        [DynamoDBProperty(Converter = typeof(DictionaryConverter))]
        public Dictionary<int, string> Theme { get; set; } = new Dictionary<int, string>();

        public string TextLanguage { get; set; }
    }
}