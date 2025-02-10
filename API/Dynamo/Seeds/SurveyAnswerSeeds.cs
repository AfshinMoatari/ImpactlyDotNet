using System;
using System.Collections.Generic;
using API.Dynamo.Seeds.Surveys;
using API.Dynamo.Seeds.Surveys.Templates;
using API.Handlers;
using API.Models.Analytics;
using API.Models.Strategy;

namespace API.Dynamo.Seeds
{
    public class SurveyAnswerSeeds
    {
        public static readonly List<EntryBatch> SurveyAnswers = new List<EntryBatch>()
        {
            GenerateWho5Answer(-6),
            GenerateWho5Answer(-5),
            GenerateWho5Answer(-4),
            GenerateWho5Answer(-3),
            GenerateWho5Answer(-2),
            GenerateWho5Answer(-1),
            GenerateWho5Answer(0),
        };

        public static EntryBatch GenerateWho5Answer(int monthsToAdd)
        {
            var choices = GenerateWho5Choices();
            double sum = 0;
            foreach (var choice in choices)
            {
                sum += choice.Value;
            }

            var answer = new EntryBatch()
            {
                Id = $"survey-answer-{monthsToAdd * -1}",
                SurveyId = Who5.Survey.Id,
                PatientId = PatientSeeds.All[0].Id,
                StrategyId = "strategy1",
                ProjectId = "admin",
                CreatedAt = DateTime.Now.AddMonths(monthsToAdd),
                AnsweredAt = DateTime.Now.AddMonths(monthsToAdd),
                Entries = choices
            };
            answer.Score = SurveyHandler.CalculateScore(answer);
            return answer;
        }

        public static List<FieldEntry> GenerateWho5Choices()
        {
            var choices = new List<FieldEntry>();

            var rnd = new Random();
            var index = 0;
            foreach (var question in Who5.Fields)
            {
                var option = Who5.Choices[rnd.Next(Who5.Choices.Count)];
                var choice = new FieldEntry()
                {
                    SurveyId = question.ParentId,
                    ParentId = question.Id,
                    Id = Guid.NewGuid().ToString(),
                    Value = (int)option.Value,
                };
                choices.Add(choice);
                index++;
            }

            return choices;
        }
    }
}