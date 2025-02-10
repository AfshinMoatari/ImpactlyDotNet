using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Strategy;

namespace API.Models.Notifications
{
    [DynamoDBTable(TableNames.Notifications)]
    public class Notification : CrudModel
    {
        public const string Prefix = "NOTIFICATIONS";

        [Newtonsoft.Json.JsonIgnore, DynamoDBHashKey]
        public override string PK { get; set; }

        [Newtonsoft.Json.JsonIgnore, DynamoDBRangeKey]
        public override string SK { get; set; }

        [Newtonsoft.Json.JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [Newtonsoft.Json.JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }
        public bool Active { get; set; }

        [DynamoDBProperty(typeof(DynamoEnumConverter<NotificationType>))]
        public NotificationType NotificationType { get; set; }
        public string DeliveryType { get; set; }
        
        public DateTime SendOutDate { get; set; }

        public DateTime? AnsweredAt { get; set; }

        public string PatientId { get; set; }

        public string StrategyId { get; set; }

        public string SurveyCode { get; set; }

        [DynamoDBIgnore]
        public IEnumerable<Survey> Surveys { get; set; }

        public string Message { get; set; }
        public string ProjectId { get; set; }
    }
}