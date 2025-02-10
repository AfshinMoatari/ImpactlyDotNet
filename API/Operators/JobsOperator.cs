using Amazon.Util;
using API.Lib;
using API.Models.Cron;
using System;
using System.Collections.Generic;
using API.Constants;

namespace API.Operators
{
    public interface IJobsOperator
    {
        public SurveyJob CreateSurveyJob(string projectId, string strategyId, string frequencyId, string patientId, string status, string cronExpression);
    }

    public class Frequent : IJobsOperator
    {
        public Frequent(){}

        public SurveyJob CreateSurveyJob(string projectId, string strategyId,
         string frequencyId, string patientId, string status, string cronExpression)
        {
            return new SurveyJob
            {
                Type = "FREQUENT",
                CronExpression = cronExpression,
                Offset = 1,
                ExecutionCount = 0,
                ProjectId = projectId,
                StrategyId = strategyId,
                FrequencyId = frequencyId,
                PatientId = patientId,
                NextExecution =
                    CronExpressionTimes.Parse(cronExpression)
                        .GetNextOccurrenceISO8601DateFormat(DateTime.UtcNow),
                Status = status
            };
        }
    }

    public class Immediate : IJobsOperator
    {
        public Immediate() { }

        public SurveyJob CreateSurveyJob(string projectId, string strategyId,
         string frequencyId, string patientId, string status, string cronExpression)
        {
            return new SurveyJob
            {
                Type = "IMMEDIATE",
                Offset = 1,
                ExecutionCount = 0,
                ProjectId = projectId,
                StrategyId = strategyId,
                FrequencyId = frequencyId,
                PatientId = patientId,
                NextExecution = DateTime.UtcNow.AddMinutes(1).ToString(Languages.ISO8601DateFormat),
                Status = status
            };
        }
    }


    public interface IJobsOperatorContext
    {
        public SurveyJob CreateSurveyJob(string searchType, string projectId, string strategyId, string frequencyId, string patientId, string status, string cronExpression);
    }

    public class JobsOperatorContext : IJobsOperatorContext
    {
        private readonly Dictionary<string, IJobsOperator> _jobsOperator = new Dictionary<string, IJobsOperator>();

        public JobsOperatorContext()
        {
            _jobsOperator.Add("Frequent", new Frequent());
            _jobsOperator.Add("Immediate", new Immediate());
        }

        public SurveyJob CreateSurveyJob(string searchType, string projectId, string strategyId,
         string frequencyId, string patientId, string status, string cronExpression)
        {
            return _jobsOperator[searchType].CreateSurveyJob(projectId, strategyId, frequencyId, patientId, status, cronExpression);
        }
    }
}
