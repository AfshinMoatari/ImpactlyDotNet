using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using API.Constants;
using API.Models.AnalyticsReport;
using API.Models.AnalyticsReport.SROI;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace API.Models.Views.Report
{
    [DynamoDBTable(TableNames.AnalyticsReport)]
    public class AnalyticsReport : CrudModel
    {
        public const string Prefix = "ANALYTICSREPORT";

        [PropertyName("PK")]
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }
        [PropertyName("SK")]
        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }
        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        [PropertyName("ReportName")] public string Name { get; set; }
        [PropertyName("ParentId")] public string ParentId { get; set; }
        [PropertyName("CreationAt")] public DateTime CreationAt { get; set; }
        [PropertyName("UpdatedAt")] public DateTime UpdatedAt { get; set; }
        [PropertyName("Type")] public string Type { get; set; }
        [DynamoDBIgnore] public string DownloadURL { get; set; }

        [PropertyName("ReportConfig")]
        [DynamoDBProperty(typeof(DynamoDBJsonConverter<SROIReportConfigV2>))]
        public SROIReportConfigV2 ReportConfig { get; set; }
     
    }


     public class DynamoDBJsonConverter<T> : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            if (value == null) return new DynamoDBNull();
            string json = JsonConvert.SerializeObject(value);
            return new Primitive(json);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is Primitive primitive)
            {
                string json = primitive.AsString();
                return JsonConvert.DeserializeObject<T>(json);
            }
            return null;
        }
    }

}