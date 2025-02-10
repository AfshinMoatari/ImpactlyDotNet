using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Auth;

namespace API.Models.Admin
{
    [DynamoDBTable(TableNames.Admins)]
    public class AdminUser : CrudModel
    {
        public const string Prefix = "USER";
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }

        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }

        public string Name { get; set; }

        public static AdminUser FromAuthUser(AuthUser user)
        {
            return new AdminUser
            {
                Id = user.Id,
                Name = user.Name
            };
        }
    }
}