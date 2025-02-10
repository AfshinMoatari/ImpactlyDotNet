using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Qoladp
    {
        public static Survey Survey = new Survey()
        {
            Id = "qoladp",
            ParentId = "TEMPLATE",
            Name = "QoL-AD (Pårørende)",
            LongName = "Quality of life in Alzheimer's Disease (Pårørende)",
            Description = "Måler livskvaliteten for borgere med demens / alzheimers og består af 13 spørgsmål (borger version)",
            Max = 12 * 4,
            Min = 0,
            Thresholds = new List<int>() {(12*4/2)},
            IsNegative = false,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "qoladpo0",
                ParentId = null,
                Index = 0,
                Text = "Dårligt",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "qoladpo1",
                ParentId = null,
                Index = 1,
                Text = "Nogenlunde",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "qoladpo2",
                ParentId = null,
                Index = 2,
                Text = "Godt",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "qoladpo3",
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
                Id = "qoladpq0",
                ParentId = "qoladp",
                Index = 0,
                Text = "Hvordan er Deres pårørendes fysiske helbred?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq1",
                ParentId = "qoladp",
                Index = 1,
                Text = "Hvordan er Deres pårørendes energi?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq2",
                ParentId = "qoladp",
                Index = 2,
                Text = "Hvordan er Deres pårørendes humør?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq3",
                ParentId = "qoladp",
                Index = 3,
                Text = "Hvordan er Deres pårørendes boligsituation?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq4",
                ParentId = "qoladp",
                Index = 4,
                Text = "Hvordan er Deres pårørendes hukommelse?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq5",
                ParentId = "qoladp",
                Index = 5,
                Text = "Hvordan har Deres pårørende det med sin familie?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq6",
                ParentId = "qoladp",
                Index = 6,
                Text = "Hvordan er Deres pårørende forhold til sin ægtefælle?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq7",
                ParentId = "qoladp",
                Index = 7,
                Text = "Hvordan er Deres pårørendes forhold til hans/hendes venner?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq8",
                ParentId = "qoladp",
                Index = 8,
                Text = "Hvordan har Deres pårørende det med sig selv?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq9",
                ParentId = "qoladp",
                Index = 9,
                Text = "Hvordan løser Deres pårørende praktiske opgaver i hjemmet?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq10",
                ParentId = "qoladp",
                Index = 10,
                Text = "Hvordan klarer Deres pårørende at gøre noget for at have det rart?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq11",
                ParentId = "qoladp",
                Index = 11,
                Text = "Hvordan er Deres pårørendes økonomi?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "qoladpq12",
                ParentId = "qoladp",
                Index = 12,
                Text = "Hvordan vil de karakterisere Deres pårørendes tilværelse som helhed?",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
