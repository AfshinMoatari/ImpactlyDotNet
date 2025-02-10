using System;
using System.Collections.Generic;
using Amazon.Util;
using API.Constants;
using API.Dynamo.Seeds.Surveys.Templates;
using API.Lib;
using API.Models.Cron;
using API.Models.Strategy;

namespace API.Dynamo.Seeds
{
    public class StrategySeeds
    {
        public static readonly List<SurveyProperty> SurveyProperties = new List<SurveyProperty>
        {
            new SurveyProperty()
            {
                ParentId = Who5.Survey.ParentId,
                Id = Who5.Survey.Id,
                Name = Who5.Survey.Name,
                Index = 0
            },

            new SurveyProperty()
            {
                ParentId = Swls1.Survey.ParentId,
                Id = Swls1.Survey.Id,
                Name = Swls1.Survey.Name,
                Index = 1
            },
        };
        
        public static readonly Strategy StrategyOne = new Strategy()
        {
            Id = "strategy1",
            ParentId = DynamoSeedAdmin.Project.Id,
            Name = "Strategy#1",
            Surveys = SurveyProperties,
        };

        public static readonly List<StrategyEffect> Effects = new List<StrategyEffect>()
        {
            new StrategyEffect {Id = "se1", Name = "Taget bussen", ParentId = StrategyOne.Id, Type = "count"},
            new StrategyEffect {Id = "se2", Name = "Løbet en tur", ParentId = StrategyOne.Id, Type = "count"},
            new StrategyEffect {Id = "se3", Name = "Kontaktet ven", ParentId = StrategyOne.Id, Type = "count"}
        };

        public static readonly BatchSendoutFrequency StrategyOneFrequency = new BatchSendoutFrequency()
        {
            Id = "freq1",
            Name = "freq1",
            ParentId = StrategyOne.Id,
            CronExpression = "0 12 1,15 * *",
            End = new End
            {
                Type = EndType.Never
            },
            Surveys = SurveyProperties,
        };

        public static readonly StrategyPatient StrategyPatient = new StrategyPatient
        {
            ParentId = StrategyOne.Id,
            Id = "patient1",
            Name = "Christian Poulsen"
        };

        public static readonly SurveyJob SurveyJob = new SurveyJob
        {
            Id = "job1",
            Type = "SurveyJob",
            CronExpression = "0 12 1,15 * *",
            ExecutionCount = 0,
            ProjectId = DynamoSeedAdmin.Project.Id,
            StrategyId = StrategyOne.Id,
            FrequencyId = StrategyOneFrequency.Id,
            PatientId = StrategyPatient.Id,
            NextExecution =
                CronExpressionTimes
                    .Parse("30 16 29 * *")
                    .GetNextOccurrence(DateTime.UtcNow)
                    .Value.DateTime.ToString(Languages.ISO8601DateFormat)
        };
    }
}