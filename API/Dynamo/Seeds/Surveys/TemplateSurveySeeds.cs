using System.Collections.Generic;
using System.Linq;
using API.Dynamo.Seeds.Surveys.Templates;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys
{
    public static class TemplateSurveySeeds
    {
        public static readonly List<string> SurveyIds = new List<string>
        {
            Who5.Survey.Id,
            Ucla3.Survey.Id,
            Pss10.Survey.Id,
            Swls.Survey.Id,
            Swls1.Survey.Id,
            Swls5.Survey.Id,
            Rcads47.Survey.Id,
            Rcads.Survey.Id,
            Gse.Survey.Id,
            Qoladb.Survey.Id,
            Qoladp.Survey.Id,
            Swemwbs.Survey.Id,
        };
        
        public static readonly List<Survey> Surveys = new List<Survey>()
        {
            Who5.Survey,
            Ucla3.Survey,
            Pss10.Survey,
            Swls.Survey,
            Swls1.Survey,
            Swls5.Survey,
            Rcads47.Survey,
            Rcads.Survey,
            Gse.Survey,
            Qoladb.Survey,
            Qoladp.Survey,
            Swemwbs.Survey,
        };
        
        private static readonly Dictionary<string, List<FieldChoice>> FieldChoices =
            new Dictionary<string, List<FieldChoice>>()
            {
                {Who5.Survey.Id, Who5.Choices},
                {Pss10.Survey.Id, Pss10.Choices},
                {Ucla3.Survey.Id, Ucla3.Choices},
                {Swls1.Survey.Id, Swls1.Choices},
                {Swls5.Survey.Id, Swls5.Choices},
                {Rcads47.Survey.Id, Rcads47.Choices},
                {Rcads.Survey.Id, Rcads.Choices},
                {Gse.Survey.Id, Gse.Choices},
                {Qoladb.Survey.Id, Qoladb.Choices},
                {Qoladp.Survey.Id, Qoladp.Choices},
                {Swemwbs.Survey.Id, Swemwbs.Choices}
            };
        
        public static readonly List<SurveyField> SurveyFields = new List<SurveyField>(){}
            .Concat(Who5.Fields)
            .Concat(Pss10.Fields)
            .Concat(Ucla3.Fields)
            .Concat(Swls1.Fields)
            .Concat(Swls5.Fields)
            .Concat(Rcads47.Fields)
            .Concat(Rcads.Fields)
            .Concat(Gse.Fields)
            .Concat(Qoladb.Fields)
            .Concat(Qoladp.Fields)
            .Concat(Swemwbs.Fields)
            .ToList();
        
        public static List<FieldChoice> GenerateSurveyChoices()
        {
            var choicesSeed = new List<FieldChoice>();
            foreach (var surveyField in SurveyFields)
            {
                for (var i = 0; i < FieldChoices[surveyField.ParentId].Count; i++)
                {
                    var efc = FieldChoices[surveyField.ParentId][i];
                    var fc = new FieldChoice
                    {
                        Index = efc.Index,
                        Text = efc.Text,
                        Value = efc.Value,
                        ParentId = surveyField.Id,
                        Id = $"{surveyField.Id}o{i}"
                    };
                    choicesSeed.Add(fc);
                }
            }
            return choicesSeed.ToList();
        }
    }
}