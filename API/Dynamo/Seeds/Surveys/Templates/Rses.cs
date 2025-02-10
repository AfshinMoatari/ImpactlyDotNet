using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public class Rses
    {
        public static Survey Survey = new Survey()
        {
            Id = "rses",
            ParentId = "TEMPLATE",
            Name = "RSES",
            LongName = "Rosenberg Self-Esteem Scale",
            Description = "Måler selvværd gennem en persons egenvurdering. Rosenbergs Self-Esteem Scale er internationalt set det mest udbredte redskab til at måle selvværd, og består af ti spørgsmål, som besvares på en firepunktsskala af borgeren selv. Fem af spørgsmålene er negativt formuleret, og fem er positivt formuleret. ",
            Max = 9 * 4,
            Min = 0,
            Thresholds = new List<int>() {(9*4/2)},
            IsNegative = false,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
            new FieldChoice()
            {
                Id = "rseso0",
                ParentId = null,
                Index = 0,
                Text = "Stærkt uenig",
                Value = 1,
            },
            new FieldChoice()
            {
                Id = "rseso1",
                ParentId = null,
                Index = 1,
                Text = "Uening",
                Value = 2,
            },
            new FieldChoice()
            {
                Id = "rseso2",
                ParentId = null,
                Index = 2,
                Text = "Enig",
                Value = 3,
            },
            new FieldChoice()
            {
                Id = "rseso3",
                ParentId = null,
                Index = 3,
                Text = "Meget enig",
                Value = 4,
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "rsesq0",
                ParentId = "rses",
                Index = 0,
                Text = "Du er alt i alt tilfreds med dig selv.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq1",
                ParentId = "rses",
                Index = 1,
                Text = "Fra tid til anden synes du, at du overhovedet ikke dur til noget.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq2",
                ParentId = "rses",
                Index = 2,
                Text = "Du synes, at du har en del gode egenskaber.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq3",
                ParentId = "rses",
                Index = 3,
                Text = "Du er i stand til at gøre noget lige så godt, som de fleste andre.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq4",
                ParentId = "rses",
                Index = 4,
                Text = "Du synes, at du har meget at være stolt af.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq5",
                ParentId = "rses",
                Index = 5,
                Text = "Du føler dig helt nyttesløst fra tid til anden.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq6",
                ParentId = "rses",
                Index = 6,
                Text = "Du synes, at du er et menneske af værdi. I det mindste lige så værdifuld som de fleste andre.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq7",
                ParentId = "rses",
                Index = 7,
                Text = "Du ville ønske, at du kunne have mere respekt for dig selv.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq8",
                ParentId = "rses",
                Index = 8,
                Text = "Alt i alt er du tilbøjelig til at tro, at du er mislykket.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
            new SurveyField()
            {
                Id = "rsesq9",
                ParentId = "rses",
                Index = 9,
                Text = "Du har en positiv holdning overfor dig selv.",
                Choices = new List<FieldChoice>()
                {
                },
                Type = "choice",
            },
        };

    }
}
