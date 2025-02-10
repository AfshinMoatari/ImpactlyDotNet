using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Analytics;
using API.Models.Strategy;

namespace API.Models.Projects
{
    [DynamoDBTable(TableNames.Patients)]
    public class ProjectPatient : CrudPropModel
    {
        public const string Prefix = "PATIENT";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Name => $"{FirstName} {LastName}";

        [DynamoDBGlobalSecondaryIndexHashKey(EmailIndex)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(PhoneNumberIndex)]
        public string PhoneNumber { get; set; }
        
        public bool IsActive { get; set; }
        public string Sex { get; set; }
        public DateTime? BirthDate { get; set; }
        public string PostalNumber { get; set; }
        public string Municipality { get; set; }
        public string Region { get; set; }
        public string StrategyName { get; set; }
        public string StrategyId { get; set; }
        public List<PatientTag> Tags { get; set; } = new List<PatientTag>();
        public DateTime LastAnswered { get; set; }
        public DateTime? SubmissionDate { get; set; }

        public bool Anonymity { get; set; }
    }
}