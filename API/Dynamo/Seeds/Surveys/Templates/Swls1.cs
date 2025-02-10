using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Swls1
    {
        public static Survey Survey = new Survey()
        {
            Id = "swls1",
            ParentId = "TEMPLATE",
            Name = "SWLS-1",
            LongName = "Satisfaction With Life Scale (1-item)",
            Description = "Måler overordnet tilfredshed med ens eget liv. SWLS er et valideret selvrapporteret mål som består af 1 spørgsmål",
            Max = 10,
            Min = 0,
            Thresholds = new List<int>() {3, 7},
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "swls1o0",
                ParentId = null,
                Index = 0,
                Text = "0",
                Value = 0,
            },
            new FieldChoice()
            {
                Id = "swls1o1",
                ParentId = null,
                Index = 1,
                Text = "1",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "swls1o2",
                ParentId = null,
                Index = 2,
                Text = "2",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "swls1o3",
                ParentId = null,
                Index = 3,
                Text = "3",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "swls1o4",
                ParentId = null,
                Index = 4,
                Text = "4",
                Value = 4,
            },
            new FieldChoice()
            {
                Id = "swls1o5",
                ParentId = null,
                Index = 5,
                Text = "5",
                Value = 5,
            },
            new FieldChoice()
            {
                Id = "swls1o6",
                ParentId = null,
                Index = 6,
                Text = "6",
                Value = 6,
            },
            new FieldChoice()
            {
                Id = "swls1o7",
                ParentId = null,
                Index = 7,
                Text = "7",
                Value = 7,
            },
            new FieldChoice()
            {
                Id = "swls1o8",
                ParentId = null,
                Index = 8,
                Text = "8",
                Value = 8,
            },
            new FieldChoice()
            {
                Id = "swls1o9",
                ParentId = null,
                Index = 9,
                Text = "9",
                Value = 9,
            },
            new FieldChoice()
            {
                Id = "swls1o10",
                ParentId = null,
                Index = 10,
                Text = "10",
                Value = 10,
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "swls1q0",
                ParentId = "swls1",
                Index = 0,
                Text = "Alt i alt, hvor tilfreds er du med dit liv for tiden? (Scoren 0 er \"slet ikke tilfreds\" og 10 er \"fuldt ud tilfreds\")",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
