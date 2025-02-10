using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Patients)]
    public class StrategyPatient : CrudPropModel
    {
        public string Name { get; set; }
        public bool Anonymity { get; set; }
    }
}