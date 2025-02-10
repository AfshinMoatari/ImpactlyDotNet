using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Auth
{
    [DynamoDBTable(TableNames.Users)]
    public class AuthUser : CrudModel
    {
        public const string Prefix = "USER";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }
        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(EmailIndex)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey(PhoneNumberIndex)]
        public string PhoneNumber { get; set; }
        [JsonIgnore] public string PasswordHashB64 { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool PrivacyPolicy { get; set; }
        public string ImageUrl { get; set; }
        public DeviceToken Device { get; set; }
        public override string Id { get; set; }

        public override DateTime CreatedAt { get; set; }
        public override DateTime UpdatedAt { get; set; }

        public string Name => $"{FirstName} {LastName}";
        public bool Active { get; set; }
        
        
        [DynamoDBIgnore] public string TextLanguage { get; set; }
    }
}