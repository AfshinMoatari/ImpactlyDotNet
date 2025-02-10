using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using API.Constants;

namespace API.Models.Strategy
{
    public class SurveyProperty
    {
        public string ParentId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public bool validated { get; set; }
        
        public string TextLanguage { get; set; }
        [DynamoDBIgnore] public IEnumerable<SurveyField> Fields { get; set; }

        public static explicit operator SurveyProperty(Survey survey) => new()
        {
            ParentId = survey.ParentId,
            Id = survey.Id,
            Name = survey.Name,
            Fields = survey.Fields,
            TextLanguage = survey.TextLanguage,
            Index = survey.Index,
        };
        public static explicit operator Survey(SurveyProperty survey) => new()
        {
            ParentId = survey.ParentId,
            Id = survey.Id,
            Name = survey.Name,
            TextLanguage = survey.TextLanguage,
            Index = survey.Index,
        };
    }
}