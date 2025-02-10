using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Swemwbs
    {
        public static Survey Survey = new Survey()
        {
            Id = "swemwbs",
            ParentId = "TEMPLATE",
            Name = "SWEMWBS",
            LongName = "Short version of the Warwick–Edinburgh Mental Wellbeing Scale",
            Description =
                "Et generisk måleinstrument, som måler positive aspekter af mental sundhed. WEMWBS måler både oplevelses- og funktionsdimensionerne af mental sundhed.",
            Max = 35,
            Min = 7,
            Thresholds = new List<int>(),
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
          new FieldChoice()
          {
              Id = "swemwbso0",
              ParentId = null,
              Index = 0,
              Text = "På intet tidspunkt",
              Value = 1,
          },
          new FieldChoice()
          {
              Id = "swemwbso1",
              ParentId = null,
              Index = 1,
              Text = "Sjældent",
              Value = 2,
          },
          new FieldChoice()
          {
              Id = "swemwbso2",
              ParentId = null,
              Index = 2,
              Text = "Noget af tiden",
              Value = 3,
          },
          new FieldChoice()
          {
              Id = "swemwbso3",
              ParentId = null,
              Index = 3,
              Text = "Ofte",
              Value = 4,
          },
          new FieldChoice()
          {
              Id = "swemwbso4",
              ParentId = null,
              Index = 4,
              Text = "Hele tiden",
              Value = 5,
          }
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "swemwbsq0",
                ParentId = "swemwbs",
                Index = 0,
                Text =
                    "Nedenfor er der en række udsagn om følelser og tanker. Kryds den boks af, der bedst svarer til, hvor ofte du har haft det sådan i løbet af de sidste 2 uger: Jeg har følt mig optimistisk i forhold til fremtiden",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq1",
                ParentId = "swemwbs",
                Index = 1,
                Text = "Jeg har følt mig nyttig",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq2",
                ParentId = "swemwbs",
                Index = 2,
                Text = "Jeg har følt mig afslappet",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq3",
                ParentId = "swemwbs",
                Index = 3,
                Text = "Jeg har klaret problemer godt",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq4",
                ParentId = "swemwbs",
                Index = 4,
                Text = "Jeg har tænkt klart",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq5",
                ParentId = "swemwbs",
                Index = 5,
                Text = "Jeg har følt mig tæt på andre mennesker",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "swemwbsq6",
                ParentId = "swemwbs",
                Index = 6,
                Text = "Jeg har været i stand til at danne min egen mening om ting",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };
    }
}