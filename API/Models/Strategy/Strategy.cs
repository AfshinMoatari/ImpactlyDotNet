using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Strategy)]
    public class Strategy : CrudPropModel
    {
        public const string Prefix = "STRATEGY";
        public string Name { get; set; }
        public List<SurveyProperty> Surveys { get; set; } = new List<SurveyProperty>();
        [DynamoDBIgnore] public IEnumerable<StrategyPatient> Patients { get; set; } = new List<StrategyPatient>();
        [DynamoDBIgnore] public IEnumerable<StrategyEffect> Effects { get; set; } = new List<StrategyEffect>();
        [DynamoDBIgnore] public IEnumerable<BatchSendoutFrequency> Frequencies { get; set; } = new List<BatchSendoutFrequency>();
    }
}