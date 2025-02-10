using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Logs;

[DynamoDBTable(TableNames.Logs)]
public class Log: CrudModel
{
    public const string Prefix = "LOG";
    
    public const string LogtypeEmail = "Email";
    public const string LogtypeSms = "SMS";
    public const string LogtypeBulkCitizen = "Bulk citizens";
    public const string LogtypeException = "Exception";

    public const string BulkImportStatusInserted = "Inserted";
    public const string BulkImportStatusSkipped = "Skipped";
    public const string BulkImportStatusUpdated = "Updated";
    
    
    [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

    [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

    [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
    public override string GSIPK { get; set; }

    [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
    public override string GSISK { get; set; }
    
    public string Type { get; set; }
    
    public string Status { get; set; }
    
    public string Sender { get; set; }
    public List<string> Receivers { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    
    public string Message { get; set; }
    
    public string ProjectId { get; set; }
}

