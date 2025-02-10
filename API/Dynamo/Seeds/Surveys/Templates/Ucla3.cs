using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Ucla3
    {
        public static Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "ucla3",
            Name = "UCLA-3",
            LongName = "UCLA Loneliness scale",
            Description = "Måler subjektive følelser af ensomhed og social isolation",
            Max = 3 * 3,
            Min = 3 * 1,
            Thresholds = new List<int>() {6},
            IsNegative = true,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>
        {
            new FieldChoice()
            {
                Id = "ucla3o1",
                Index = 1,
                Text = "Sjældent",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "ucla3o2",
                Index = 2,
                Text = "En gang imellem",
                Value = 2
            },
            new FieldChoice()
            {
                Id = "ucla3o3",
                Index = 3,
                Text = "Ofte",
                Value = 3
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "ucla3q1",
                ParentId = "ucla3",
                Index = 1,
                Text = "Hvor ofte føler du dig isoleret fra andre?",
            },
            new SurveyField()
            {
                Id = "ucla3q2",
                ParentId = "ucla3",
                Index = 2,
                Text = "Hvor ofte føler du, at du savner nogen at være sammen med?",
            },
            new SurveyField()
            {
                Id = "ucla3q3",
                ParentId = "ucla3",
                Index = 3,
                Text = "Hvor ofte føler du dig holdt udenfor?",
            },
        };
    }
}