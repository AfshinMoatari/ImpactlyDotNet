using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public static class Pss10
    {
        public static Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "pss10",
            Name = "PSS-10",
            LongName = "Perceived Stress Scale (PSS-10)",
            Description =
                "Måler i hvilken grad respondenten oplevet, at livet er uforudsigeligt, ukontrollerbart og overvældende. Fokuserer på hvordan patienten oplever at kunne håndtere hverdagen og ikke på specifikke forhold. Bliver brugt til at vurdere stress i større befolkningsgrupper, fordi det er et uspecifikt mål for oplevet stress. Kan ikke stille en diagnose, men kan give et praj om hvad patientens symptomer handler om",
            Max = 10 * 4,
            Min = 0,
            Thresholds = new List<int>() {14, 27},
            IsNegative = true,
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>
        {
            new FieldChoice()
            {
                Id = "pss10o0",
                Index = 0,
                Text = "Aldrig",
                Value = 0
            },
            new FieldChoice()
            {
                Id = "pss10o1",
                Index = 1,
                Text = "Næsten aldrig",
                Value = 1
            },
            new FieldChoice()
            {
                Id = "pss10o2",
                Index = 2,
                Text = "Ind imellem",
                Value = 2
            },
            new FieldChoice()
            {
                Id = "pss10o3",
                Index = 3,
                Text = "Ret ofte",
                Value = 3
            },
            new FieldChoice()
            {
                Id = "pss10o4",
                Index = 4,
                Text = "Meget ofte",
                Value = 4
            },
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
            new SurveyField()
            {
                Id = "pss10q0",
                ParentId = "pss10",
                Index = 0,
                Text = "Hvor ofte inden for den sidste måned er du blevet oprevet over noget, der skete uventet?"
            },
            new SurveyField()
            {
                Id = "pss10q1",
                ParentId = "pss10",
                Index = 1,
                Text =
                    "Hvor ofte inden for den sidste måned har du følt, at du ikke kunne kontrollere de betydningsfulde ting i dit liv?"
            },
            new SurveyField()
            {
                Id = "pss10q2",
                ParentId = "pss10",
                Index = 2,
                Text = "Hvor ofte inden for den sidste måned har du følt dig nervøs og \"stresset\"?"
            },
            new SurveyField()
            {
                Id = "pss10q3",
                ParentId = "pss10",
                Index = 3,
                Text =
                    "Hvor ofte inden for den sidste måned har du følt dig sikker på din evne til at klare dine personlige problemer?"
            },
            new SurveyField()
            {
                Id = "pss10q4",
                ParentId = "pss10",
                Index = 4,
                Text = "Hvor ofte inden for den sidste måned har du følt, at tingene gik, som du gerne ville have det?"
            },
            new SurveyField()
            {
                Id = "pss10q5",
                ParentId = "pss10",
                Index = 5,
                Text =
                    "Hvor ofte inden for den sidste måned har du oplevet, at du ikke kunne overkomme alle de ting du skulle?"
            },
            new SurveyField()
            {
                Id = "pss10q6",
                ParentId = "pss10",
                Index = 6,
                Text =
                    "Hvor ofte inden for den sidste måned har du været i stand til at håndtere dagligdags irritations-momenter"
            },
            new SurveyField()
            {
                Id = "pss10q7",
                ParentId = "pss10",
                Index = 7,
                Text = "Hvor ofte inden for den sidste måned har du følt, at du havde styr på tingene?"
            },
            new SurveyField()
            {
                Id = "pss10q8",
                ParentId = "pss10",
                Index = 8,
                Text = "Hvor ofte inden for den sidste måned er du blevet vred over ting, du ikke har indflydelse på?"
            },
            new SurveyField()
            {
                Id = "pss10q9",
                ParentId = "pss10",
                Index = 9,
                Text =
                    "Hvor ofte inden for den sidste måned har du følt, at dine problemer hobede sig så meget op, at du ikke kunne klare dem?"
            },
        };
    }
}