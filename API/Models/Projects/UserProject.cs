using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Projects
{
    [DynamoDBTable(TableNames.Projects)]
    public class UserProject : CrudPropModel
    {
        public string Name { get; set; }
    }
}