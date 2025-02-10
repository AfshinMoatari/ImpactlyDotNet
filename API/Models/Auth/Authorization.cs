using System;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Auth
{
    [DynamoDBTable(TableNames.Authorizations)]
    public class Authorization
    {
        [DynamoDBHashKey] public string Id { get; set; }
        public string TokenType { get; set; }
        
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string TextLanguage { get; set; }
    }
}