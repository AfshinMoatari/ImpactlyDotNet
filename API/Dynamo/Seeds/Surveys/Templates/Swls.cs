using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Swls
    {
        public static Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "swls",
            Name = "SWLS",
            LongName = "Satisfaction With Life Scale",
            Description =
                "Måler overordnet tilfredshed med ens eget liv. SWLS er et valideret selvrapporteret mål som består af 5 spørgsmål",
            Max = 5 * 7,
            Min = 5 * 1,
            Thresholds = new List<int>() {15, 25},
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>
        {
            new FieldChoice()
            {
                Id = "swlso0",
                Index = 0,
                Text = "Meget uenig",
                Value = 1
            },
            new FieldChoice()
            {
                Id = "swlso1",
                Index = 1,
                Text = "Uenig",
                Value = 2
            },
            new FieldChoice()
            {
                Id = "swlso2",
                Index = 2,
                Text = "Lidt uenig",
                Value = 3
            },
            new FieldChoice()
            {
                Id = "swlso3",
                Index = 3,
                Text = "Hverken enig eller uenig",
                Value = 4
            },
            new FieldChoice()
            {
                Id = "swlso4",
                Index = 4,
                Text = "Lidt enig",
                Value = 5
            },
            new FieldChoice()
            {
                Id = "swlso5",
                Index = 5,
                Text = "Enig",
                Value = 6
            },
            new FieldChoice()
            {
                Id = "swlso6",
                Index = 6,
                Text = "Meget enig",
                Value = 7
            }
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "swlsq0",
                ParentId = "swls",
                Index = 0,
                Text = "På de fleste områder er mit liv tæt op ad mit ideal",
            },
            new SurveyField()
            {
                Id = "swlsq1",
                ParentId = "swls",
                Index = 1,
                Text = "Omstændighederne i mit liv er glimrende",
            },
            new SurveyField()
            {
                Id = "swlsq2",
                ParentId = "swls",
                Index = 2,
                Text = "Jeg er tilfreds med mit liv",
            },
            new SurveyField()
            {
                Id = "swlsq3",
                ParentId = "swls",
                Index = 3,
                Text = "Indtil nu har jeg fået de vigtige ting, som jeg ønsker i mit liv",
            },
            new SurveyField()
            {
                Id = "swlsq4",
                ParentId = "swls",
                Index = 4,
                Text = "Hvis jeg kunne leve mit liv forfra, ville jeg næsten intet forandre",
            },
        };
    }
}