using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Dynamo.Seeds;

namespace API.Models.Projects
{
    [DynamoDBTable(TableNames.Projects)]
    public class ProjectUser : CrudPropModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => $"{FirstName} {LastName}";

        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber)] public string PhoneNumber { get; set; }
        
        //public string RoleId { get; set; } = RoleSeeds.StandardRole.Id;
        
        public string RoleId { get; set; }
        
    }
}