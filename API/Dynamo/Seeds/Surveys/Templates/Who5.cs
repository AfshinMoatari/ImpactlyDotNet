using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public static class Who5
    {
        public static readonly Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "who5",
            Name = "WHO-5",
            LongName = "The World Health Organisation - Five Well-Being Index",
            Description =
                "Måler mental sundhed ud fra trivsel og velbefindende. Både et måleinstrument for generel mental sundhed, eller som screeningsværktøj for risiko for depression",
            Max = (5 * 5) * 4,
            Min = 0,
            Thresholds = new List<int>() {36, 50},
        };

        public static readonly List<FieldChoice> Choices = new List<FieldChoice>
        {
            new FieldChoice()
            {
                Id = "who5o0",
                Index = 0,
                Text = "På intet tidspunkt",
                Value = 0
            },
            new FieldChoice()
            {
                Id = "who5o1",
                Index = 1,
                Text = "Lidt af tiden",
                Value = 1
            },
            new FieldChoice()
            {
                Id = "who5o2",
                Index = 2,
                Text = "Lidt mindre end halvdelen af tiden",
                Value = 2
            },
            new FieldChoice()
            {
                Id = "who5o3",
                Index = 3,
                Text = "Lidt mere end halvdelen af tiden",
                Value = 3
            },
            new FieldChoice()
            {
                Id = "who5o4",
                Index = 4,
                Text = "Det meste af tiden",
                Value = 4
            },
            new FieldChoice()
            {
                Id = "who5o5",
                Index = 5,
                Text = "Hele tiden",
                Value = 5
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "who5q0",
                ParentId = "who5",
                Index = 0,
                Text = "I de sidste 2 uger har jeg været glad og i godt humør",
            },
            new SurveyField()
            {
                Id = "who5q1",
                ParentId = "who5",
                Index = 1,
                Text = "I de sidste 2 uger har jeg følt mig rolig og afslappet",
            },
            new SurveyField()
            {
                Id = "who5q2",
                ParentId = "who5",
                Index = 2,
                Text = "I de sidste 2 uger har jeg følt mig aktiv og energisk",
            },
            new SurveyField()
            {
                Id = "who5q3",
                ParentId = "who5",
                Index = 3,
                Text = "I de sidste 2 uger er jeg vågnet frisk og udhvilet",
            },
            new SurveyField()
            {
                Id = "who5q4",
                ParentId = "who5",
                Index = 4,
                Text = "I de sidste 2 uger har min dagligdag været fyldt med ting der interesserer mig",
            },
        };
    };
}