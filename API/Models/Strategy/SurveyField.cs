using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    [DynamoDBTable(TableNames.Strategy)]
    public class SurveyField : CrudPropModel
    {
        public const string Prefix = "FIELD";
        public int Index { get; set; }
        public string Text { get; set; }
        public string Type { get; set; } = "choice";

        public string ScaleCategory { get; set; }
        public int LikertScaleChoiceAmount { get; set; }
        public bool SkipNeutral { get; set; }
        public bool IsNonMandatory { get; set; }
        public bool isMultipleChoices { get; set; }

        public string Language { get; set; }

        public int FieldIndex => Index;
        
        public string FieldTemplateId { get; set; }

        public FieldValidation Validation { get; set; } = new FieldValidation
        {
            Required = true
        };

        [DynamoDBIgnore] public IEnumerable<FieldChoice> Choices { get; set; } = new List<FieldChoice>();
    }
}