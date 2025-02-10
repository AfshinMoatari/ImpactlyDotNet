using System;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Util;
using API.Constants;
using API.Lib;
using Nest;

namespace API.Models.Cron
{
    [DynamoDBTable(TableNames.Jobs)]
    public class SurveyJob : CrudModel
    {
        public const string Prefix = "JOB";
        public const string GlobalSecondaryIndex2 = "SK-GSISK-index-2";
        public const string GlobalSecondaryIndex3 = "SK-GSISK-index-3";


        
        [JsonIgnore, DynamoDBHashKey] public override string PK { get; set; }
        [JsonIgnore, DynamoDBRangeKey] public override string SK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex)]
        public override string GSIPK { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex)]
        public override string GSISK { get; set; }
        
        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex2)]
        public string GSIPK2 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex2)]
        public string GSISK2 { get; set; }
        
        [JsonIgnore, DynamoDBGlobalSecondaryIndexHashKey(GlobalSecondaryIndex3)]
        public string GSIPK3 { get; set; }

        [JsonIgnore, DynamoDBGlobalSecondaryIndexRangeKey(GlobalSecondaryIndex3)]
        public string GSISK3 { get; set; }
        
        public string ProjectId { get; set; }
        public string StrategyId { get; set; }
        public string FrequencyId { get; set; }
        public string PatientId { get; set; }
        
        public string Type { get; set; }

        public string CronExpression { get; set; }

        public string NextExecution { get; set; }

        public int Offset { get; set; } = 1;

        public int ExecutionCount { get; set; }

        public string Status { get; set; }

        public DateTime? LastUpdated { get; set; }

        public string InstanceId { get; set; }

        public static SurveyJob CreateSurveyJob(string cronExpression, string projectId, string strategyId,
            string frequencyId, string patientId)
        {
            return new SurveyJob
            {
                Type = "SurveyJob",
                CronExpression = cronExpression,
                Offset = 1,
                ExecutionCount = 0,
                ProjectId = projectId,
                StrategyId = strategyId,
                FrequencyId = frequencyId,
                PatientId = patientId,
                NextExecution =
                    CronExpressionTimes.Parse(cronExpression)
                        .GetNextOccurrenceISO8601DateFormat(DateTime.UtcNow)
            };
        }

        public static SurveyJob CreateSurveyJobWithType(string cronExpression, string projectId, string strategyId,
            string frequencyId, string patientId, string type, string status)
        {
            return new SurveyJob
            {
                Type = type,
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

        public static SurveyJob CreateImmediateSurveyJob(string projectId, string strategyId,
         string frequencyId, string patientId, string status)
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
}