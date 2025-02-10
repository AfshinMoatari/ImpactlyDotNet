using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Gse
    {
        public static Survey Survey = new Survey()
        {
            Id = "gse",
            ParentId = "TEMPLATE",
            Name = "GSE",
            LongName = "General Self-Efficacy Scale",
            Description = "Måler ens egenvurdering af sine evner og ressourcer til at håndtere udfordringer i livet. Skalaen tager udgangspunkt i forståelsen af, at man selv er ansvarlig for succesfulde outcomes. Den består af 10 spørgsmål.",
            Max = 9 * 5,
            Min = 0,
            Thresholds = new List<int>() {(9*5/2)},
            IsNegative = false,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "gseo0",
                ParentId = null,
                Index = 0,
                Text = "Passer slet ikke",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "gseo1",
                ParentId = null,
                Index = 1,
                Text = "Passer en smule",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "gseo2",
                ParentId = null,
                Index = 2,
                Text = "Passer nogenlunde",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "gseo3",
                ParentId = null,
                Index = 3,
                Text = "Passer præcist",
                Value = 4,
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "gseq0",
                ParentId = "gse",
                Index = 0,
                Text = "Jeg kan altid løse vanskelige problemer, hvis jeg prøver ihærdigt nok",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq1",
                ParentId = "gse",
                Index = 1,
                Text = "Hvis nogen modarbejder mig, finder jeg en måde til at opnå det, jeg vil",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq2",
                ParentId = "gse",
                Index = 2,
                Text = "Det er let for mig at holde fast ved mine planer og realisere mine mål",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq3",
                ParentId = "gse",
                Index = 3,
                Text = "Jeg er sikker på, at jeg kan håndtere uventede hændelser",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq4",
                ParentId = "gse",
                Index = 4,
                Text = "Takket være mine personlige ressourcer, ved jeg, hvordan jeg skal klare uforudsete situationer",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq5",
                ParentId = "gse",
                Index = 5,
                Text = "Jeg kan løse de fleste problemer, hvis jeg yder den nødvendige indsats",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq6",
                ParentId = "gse",
                Index = 6,
                Text = "Jeg bevarer roen, når der er problemer, da jeg stoler på mine evner til at løse dem",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq7",
                ParentId = "gse",
                Index = 7,
                Text = "Når jeg støder på et problem, kan jeg som regel finde flere løsninger",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq8",
                ParentId = "gse",
                Index = 8,
                Text = "Hvis jeg er i vanskeligheder, kan jeg som regel finde en udvej",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "gseq9",
                ParentId = "gse",
                Index = 9,
                Text = "Lige meget hvad der sker, kan jeg som regel klare det",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
