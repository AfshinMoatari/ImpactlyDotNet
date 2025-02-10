using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Qoladb
    {
        public static Survey Survey = new Survey()
        {
            Id = "qoladb",
            ParentId = "TEMPLATE",
            Name = "QoL-AD (Borger)",
            LongName = "Quality of life in Alzheimer's Disease (Borger)",
            Description = "Måler livskvaliteten for borgere med demens / alzheimers og består af 13 spørgsmål (pårørende version)",
            Max = 12 * 4,
            Min = 0,
            Thresholds = new List<int>() {(12*4/2)},
            IsNegative = false,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "qoladbo0",
                ParentId = null,
                Index = 0,
                Text = "Dårligt",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "qoladbo1",
                ParentId = null,
                Index = 1,
                Text = "Nogenlunde",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "qoladbo2",
                ParentId = null,
                Index = 2,
                Text = "Godt",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "qoladbo3",
                ParentId = null,
                Index = 3,
                Text = "Udmærket",
                Value = 4,
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "qoladbq0",
                ParentId = "qoladb",
                Index = 0,
                Text = "Hvordan er dit fysiske helbred?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq1",
                ParentId = "qoladb",
                Index = 1,
                Text = "Hvordan er dit energiniveau?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq2",
                ParentId = "qoladb",
                Index = 2,
                Text = "Hvordan er dit humør?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq3",
                ParentId = "qoladb",
                Index = 3,
                Text = "Hvordan vil du beskrive din boligsituation?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq4",
                ParentId = "qoladb",
                Index = 4,
                Text = "Hvordan er din hukommelse?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq5",
                ParentId = "qoladb",
                Index = 5,
                Text = "Hvordan er dit forhold til din familie?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq6",
                ParentId = "qoladb",
                Index = 6,
                Text = "Hvordan er dig forhold til din ægtefælle?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq7",
                ParentId = "qoladb",
                Index = 7,
                Text = "Hvordan vil du beskrive dit forhold til dine venner?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq8",
                ParentId = "qoladb",
                Index = 8,
                Text = "Hvordan har du det med dig selv?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq9",
                ParentId = "qoladb",
                Index = 9,
                Text = "Hvordan vil du beskrive din evne til at udføre praktiske opgaver i hjemmet?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq10",
                ParentId = "qoladb",
                Index = 10,
                Text = "Er du god til at gøre noget godt for dig selv?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq11",
                ParentId = "qoladb",
                Index = 11,
                Text = "Hvordan er din økonomi?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladbq12",
                ParentId = "qoladb",
                Index = 12,
                Text = "Hvordan vil du karakterisere din tilværelse som helhed?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
