using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Util;
using API.Extensions;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Views.Report;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static OpenAI.GPT3.ObjectModels.Models;

namespace API.Repositories
{
    public interface IAnalyticsReportContext : ICrudRepository<AnalyticsReport>
    {
        Task<List<AnalyticsReport>> ReadAllByProjectId(string projectId);
        Task<AnalyticsReport> WriteAnalyticsReport(AnalyticsReport analyticsReport);
        Task DeleteAnalyticsReport(string projectId, string analyticsReportId);
        Task<AnalyticsReport> ReadByPKProjectIdAndAnalyticsReportId(string projectId, string analyticsReportId);
        Task<AnalyticsReport> UpdateAnalyticsReport(AnalyticsReport analyticsReport);
    }

    public class AnalyticsReportContext : CrudRepository<AnalyticsReport>, IAnalyticsReportContext
    {
        public AnalyticsReportContext(IAmazonDynamoDB client) : base(client) { }

        public override string ParentPrefix => Project.Prefix;
        public override string ModelPrefix => AnalyticsReport.Prefix;

        public override AnalyticsReport ToDynamoItem(AnalyticsReport model)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = model.ParentId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.SK = SortKeyById(model.Id);
            model.PK = PartitionKeyByParentId(model.ParentId);
            model.GSIPK = $"PROJECT#{model.ParentId}#TYPE#{model.Type}";
            model.GSISK = $"{model.Type}#{model.CreatedAt.ToUniversalTime().ToString(AWSSDKUtils.ISO8601DateFormat)}";

            return model;
        }

        public async Task<List<AnalyticsReport>> ReadAllByProjectId(string projectId)
        {
            var PK = ParentPrefix + "#" + projectId;
            var operationConfig = new DynamoDBOperationConfig();
            var queryResults = await Context.QueryAsync<AnalyticsReport>(
                PK,
                operationConfig
            ).GetRemainingAsync();

            return queryResults;
        }

        public async Task<AnalyticsReport> ReadByPKProjectIdAndAnalyticsReportId(string projectId, string analyticsReportId)
        {
            var PK = ParentPrefix + "#" + projectId;
            var SK = ModelPrefix + "#" + analyticsReportId;

            return await Context.LoadAsync<AnalyticsReport>(PK, SK);
        }

        public async Task<AnalyticsReport> WriteAnalyticsReport(AnalyticsReport analyticsReport)
        {
            var newAnalyticsReport = ToDynamoItem(analyticsReport);
            await Context.SaveAsync(newAnalyticsReport);
            return newAnalyticsReport;
        }

        public async Task<AnalyticsReport> UpdateAnalyticsReport(AnalyticsReport analyticsReport)
        {
            analyticsReport.UpdatedAt = DateTime.Now;
            await Context.SaveAsync(analyticsReport);
            return analyticsReport;
        }


        public async Task DeleteAnalyticsReport(string projectId, string analyticsReportId)
        {
            var PK = ParentPrefix + "#" + projectId;
            var SK = ModelPrefix + "#" + analyticsReportId;

            await Context.DeleteAsync<AnalyticsReport>(PK, SK);
        }
    }
}
