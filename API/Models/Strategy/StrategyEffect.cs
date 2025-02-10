using System;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Strategy)]
    public class StrategyEffect : CrudPropModel
    {
        public const string Prefix = "EFFECT";
        public string Name { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        
        public int Index { get; set; }

        public DateTime? DeletedAt { get; set; } = null;
        
        
    }
}