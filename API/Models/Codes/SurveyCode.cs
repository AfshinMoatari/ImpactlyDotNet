using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;
using API.Models.Strategy;

namespace API.Models.Codes
{
    [DynamoDBTable(TableNames.Codes)]
    public class SurveyCode : CrudModel
    {
        public const string Prefix = "SURVEY";
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public string PatientId { get; set; }
        public string FrequencyId { get; set; }
        public List<SurveyProperty> Properties { get; set; }
        [DynamoDBIgnore] public List<Survey> Surveys { get; set; }
    }
}