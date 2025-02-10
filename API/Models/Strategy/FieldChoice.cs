using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    // TODO Denormalization of properties of choice
    [DynamoDBTable(TableNames.Strategy)]
    public class FieldChoice : CrudPropModel
    {
        public const string Prefix = "CHOICE";
        public int Index { get; set; }
        public string Text { get; set; }
        
        public string TextLanguage { get; set; }
        public double Value { get; set; }
    }
}