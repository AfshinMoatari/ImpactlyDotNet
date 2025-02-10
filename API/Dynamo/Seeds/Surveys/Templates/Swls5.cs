using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Swls5
    {
        public static Survey Survey = new Survey()
        {
            Id = "swls5",
            ParentId = "TEMPLATE",
            Name = "SWLS-5",
            LongName = "Satisfaction With Life Scale (5-items)",
            Description = "Måler overordnet tilfredshed med ens eget liv. SWLS er et valideret selvrapporteret mål som består af 5 spørgsmål",
            Max = 5 * 7,
            Min = 5 * 1,
            Thresholds = new List<int>() {15, 25},
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "swls5o0",
                ParentId = null,
                Index = 0,
                Text = "Meget uenig",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "swls5o1",
                ParentId = null,
                Index = 1,
                Text = "Uening",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "swls5o2",
                ParentId = null,
                Index = 2,
                Text = "Lidt uening",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "swls5o3",
                ParentId = null,
                Index = 3,
                Text = "Hverken enig eller uenig",
                Value = 4,
            },
            new FieldChoice()
            {
                Id = "swls5o4",
                ParentId = null,
                Index = 4,
                Text = "Lidt enig",
                Value = 5,
            },
            new FieldChoice()
            {
                Id = "swls5o5",
                ParentId = null,
                Index = 5,
                Text = "Enig",
                Value = 6,
            },
            new FieldChoice()
            {
                Id = "swls5o6",
                ParentId = null,
                Index = 6,
                Text = "Meget enig",
                Value = 7,
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "swls5q0",
                ParentId = "swls5",
                Index = 0,
                Text = "På de fleste områder er mit liv tæt op ad mit ideal",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swls5q1",
                ParentId = "swls5",
                Index = 1,
                Text = "Omstændighederne i mit liv er glimrende",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swls5q2",
                ParentId = "swls5",
                Index = 2,
                Text = "Jeg er tilfreds med mit liv",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swls5q3",
                ParentId = "swls5",
                Index = 3,
                Text = "Indtil nu har jeg fået de vigtige ting, som jeg ønsker i mit liv",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swls5q4",
                ParentId = "swls5",
                Index = 4,
                Text = "Hvis jeg kunne leve mit liv forfra, ville jeg næsten intet forandre",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
