using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Dynamo.Seeds;

namespace API.Models.Projects
{
    public class UpdateSroiRequest
    {
        [Required]
        public string Url { get; set; }
    }
}