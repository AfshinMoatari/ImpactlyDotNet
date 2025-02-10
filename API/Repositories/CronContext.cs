using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Util;
using API.Constants;
using API.Extensions;
using API.Models.Analytics;
using API.Models.Cron;
using API.Models.Projects;
using API.Models.Strategy;

namespace API.Repositories
{
    public interface ICronContext
    {
        ISurveyJobRepository SurveyJobs { get; }
    }

    public class CronContext : ICronContext
    {
        public ISurveyJobRepository SurveyJobs { get; }

        public CronContext(IAmazonDynamoDB client)
        {
            SurveyJobs = new SurveyJobRepository(client);
        }
    }

    public interface ISurveyJobRepository : ICrudRepository<SurveyJob>
    {
        Task<List<SurveyJob>> ReadAll(SurveyJobAccess access);
        Task<List<SurveyJob>> ReadBetween(RangeFilter access);
    }

    public class SurveyJobRepository : CrudRepository<SurveyJob>, ISurveyJobRepository
    {
        public override string Prefix => SurveyJob.Prefix;

        public override string SortKeyValue(SurveyJob model) => model.NextExecution;

        public override string GSIPartitionKey(SurveyJob model) =>
            $"{ModelPrefix}#META";

        public override string GSISortKey(SurveyJob model) =>
            $"{ModelPrefix}#{SortKeyValue(model)}";

        public override SurveyJob ToDynamoItem(SurveyJob model)
        {
            return ToDynamoItem(Metakey, model);
        }

        public override SurveyJob ToDynamoItem(string parentId, SurveyJob model)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = parentId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = GSISortKey(model);
            model.GSIPK2 = $"{GSIPartitionKey(model)}#{ProjectPatient.Prefix}#{model.PatientId}";
            model.GSISK2 = GSISortKey(model);
            model.GSIPK3 = $"{GSIPartitionKey(model)}#{Strategy.Prefix}#{model.StrategyId}";
            model.GSISK3 = GSISortKey(model);
            return model;
        }

        public override async Task<SurveyJob> Update(string parentId, SurveyJob model)
        {
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = GSISortKey(model);
            model.GSIPK2 = $"{GSIPartitionKey(model)}#{ProjectPatient.Prefix}#{model.ProjectId}";
            model.GSISK2 = GSISortKey(model);
            model.GSIPK3 = $"{GSIPartitionKey(model)}#{Strategy.Prefix}#{model.StrategyId}";
            model.GSISK3 = GSISortKey(model);
            await Context.SaveAsync(model);
            return model;
        }

        public override async Task<SurveyJob> UpdateValue(string id, Action<SurveyJob> update)
        {
            var model = await Read(id);
            update.Invoke(model);
            return await Update(Metakey, model);
        }

        public SurveyJobRepository(IAmazonDynamoDB client) : base(client)
        {
        }

        public async Task<List<SurveyJob>> ReadAll(SurveyJobAccess access)
        {
            var readAsStrategy = !string.IsNullOrEmpty(access.StrategyId);
            var searchRequiredComposition =
                $"{ModelPrefix}#META" +
                (readAsStrategy
                    ? $"#{Strategy.Prefix}#{access.StrategyId}"
                    : $"#{ProjectPatient.Prefix}#{access.PatientId}");

            var indexName = readAsStrategy ? SurveyJob.GlobalSecondaryIndex3 : SurveyJob.GlobalSecondaryIndex2;

            return await Context.QueryAsync<SurveyJob>(
                searchRequiredComposition,
                QueryOperator.BeginsWith,
                new[] {$"{ModelPrefix}"},
                new DynamoDBOperationConfig
                {
                    IndexName = indexName
                }
            ).GetRemainingAsync();
        }

        public async Task<List<SurveyJob>> ReadBetween(RangeFilter access)
        {
            return await Context.QueryAsync<SurveyJob>(
                $"{ModelPrefix}#META",
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}"
                },
                new DynamoDBOperationConfig
                {
                    IndexName = GlobalSecondaryIndex
                }
            ).GetRemainingAsync();
        }
    }
}