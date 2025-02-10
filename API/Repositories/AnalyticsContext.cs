using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Util;
using API.Constants;
using API.Extensions;
using API.Handlers;
using API.Models;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Strategy;
using Microsoft.IdentityModel.Tokens;


namespace API.Repositories
{
    public interface IAnalyticsContext
    {
        IEntryBatchRepository EntryBatches { get; }
        IFieldEntriesRepository FieldEntries { get; }
        IRegistrationRepository Registrations { get; }

        // Methods
        Task<EntryBatch> CreateEntries(EntryBatch batch);

        Task<List<Registration>> FindRegistrationsByEffectId(string projectId, string effectId);
        Task<List<Registration>> FindBeforeByEffectId(string projectId, string effectId);

        Task DeleteTag(string projectId, string patientId, string tagName);

        Task AddTags(string projectId, string patientId, IEnumerable<string> tags);
        
    }

    public class AnalyticsContext : IAnalyticsContext
    {
        public IEntryBatchRepository EntryBatches { get; }
        public IFieldEntriesRepository FieldEntries { get; }
        public IRegistrationRepository Registrations { get; }


        public AnalyticsContext(IAmazonDynamoDB client)
        {
            EntryBatches = new EntryBatchRepository(client);
            FieldEntries = new FieldEntriesRepository(client);
            Registrations = new RegistrationRepository(client);
        }

        public async Task<List<Registration>> FindRegistrationsByEffectId(string projectId, string effectId)
        {
            return await Registrations.ReadByEffectId(projectId, effectId);
        }
        public async Task<List<Registration>> FindBeforeByEffectId(string projectId, string effectId)
        {
            return await Registrations.ReadBeforeById(projectId, effectId);
        }

        public async Task<EntryBatch> CreateEntries(EntryBatch batch)
        {
            batch.Score = batch.Entries.Sum(e => e.Value);
            batch.Score = SurveyHandler.CalculateScore(batch);
            batch.AverageScore = SurveyHandler.CalculateAverageScore(batch);

            var newEntryBatch = EntryBatches.ToDynamoItem(batch);
            var entryBatch = EntryBatches.Context.CreateBatchWrite<EntryBatch>(
                new DynamoDBOperationConfig {SkipVersionCheck = true, IgnoreNullValues = true});
            entryBatch.AddPutItem(newEntryBatch);

            var fieldEntryBatch = EntryBatches.Context.CreateBatchWrite<FieldEntry>(
                new DynamoDBOperationConfig {SkipVersionCheck = true, IgnoreNullValues = true});
            fieldEntryBatch.AddPutItems(
                batch.Entries.Select(e => FieldEntries.ToDynamoItem(newEntryBatch.Id, e)));

            await EntryBatches.Context.ExecuteBatchWriteAsync(
                new BatchWrite[]
                {
                    entryBatch,
                    fieldEntryBatch,
                }
            );

            return batch;
        }

        public async Task DeleteTag(string projectId, string patientId, string tagName)
        {
            var batches = await EntryBatches.ReadByTag(projectId, patientId, tagName);
            if (batches != null && batches.Any())
            {
                foreach (var entryBatch in batches.Where(entryBatch => entryBatch.Tags.Remove(tagName)))
                {
                    await EntryBatches.Delete(entryBatch.Id);
                    await EntryBatches.Create(entryBatch);
                }
            }

            var fields = await FieldEntries.ReadByTag(projectId, patientId, tagName);
            if (fields != null && fields.Any())
            {
                foreach (var field in fields.Where(field => field.Tags.Remove(tagName)))
                {
                    await FieldEntries.Delete(field.ParentId, field.Id);
                    await FieldEntries.Create(field.ParentId, field);

                }
            }

            var registrations = await Registrations.ReadByTag(projectId, patientId, tagName);
            if (registrations != null && registrations.Any())
            {
                foreach (var registration in registrations.Where(registration => registration.Tags.Remove(tagName)))
                {
                    await Registrations.Delete(registration.Id);
                    await Registrations.Create(registration);
                }
            }


        }

        public async Task AddTags(string projectId, string patientId, IEnumerable<string> tags)
        {
            var batchEntries = await EntryBatches.ReadBetween(new SurveyAccess()
            {
                ProjectId = projectId,
                PatientId = patientId,
                SearchStart = DateTime.MinValue,
                SearchEnd = DateTime.MaxValue
            });
            if (batchEntries != null && batchEntries.Any())
            {
                foreach (var entry in batchEntries)
                {
                    entry.Tags ??= new List<string>();
                    foreach (var tagItem in tags.ToList().Where(tagItem => !entry.Tags.Contains(tagItem)))
                    {
                        entry.Tags.Add(tagItem);
                        await EntryBatches.Delete(entry.Id);
                        await EntryBatches.Create(entry);
                    }
                }
            }

            var registrations = await Registrations.ReadBetween(new RegistrationAccess()
            {
                ProjectId = projectId,
                PatientId = patientId,
                SearchStart = DateTime.MinValue,
                SearchEnd = DateTime.MaxValue
            });
            if (registrations != null && registrations.Any())
            {
                foreach (var registration in registrations)
                {
                    registration.Tags ??= new List<string>();
                    foreach (var tagItem in tags.ToList().Where(tagItem => !registration.Tags.Contains(tagItem)))
                    {
                        registration.Tags.Add(tagItem);
                        await Registrations.Delete(registration.Id);
                        await Registrations.Create(registration);
                    }
                }

            }

            var fields = await FieldEntries.ReadByPatient(projectId, patientId);
            if (fields!=null && fields.Any())
            {
                foreach (var field in fields)
                {
                    field.Tags ??= new List<string>();
                    foreach (var tagItem in tags.ToList().Where(tagItem => !field.Tags.Contains(tagItem)))
                    {
                        field.Tags.Add(tagItem);
                        await FieldEntries.Delete(field.ParentId, field.Id);
                        await FieldEntries.Create(field.ParentId, field);
                    }
                }
            }
        }
        
        
    }


    public interface IEntryBatchRepository : ICrudRepository<EntryBatch>
    {
        Task<List<EntryBatch>> ReadBetween(SurveyAccess access);
        Task<List<EntryBatch>> ReadByTag(string projectId, string patientId, string tag);
        Task<List<EntryBatch>> ReadBetweenSurveyByStrategyIdAndSurveyId(SurveyAccess access);

    }

    public class EntryBatchRepository : CrudRepository<EntryBatch>, IEntryBatchRepository
    {
        public override string ParentPrefix => EntryBatch.Prefix;
        public override string ModelPrefix => EntryBatch.Prefix;

        public override string GSIPartitionKey(EntryBatch model) =>
            $"{ModelPrefix}#META#{Project.Prefix}#{model.ProjectId}";

        public override string SortKeyValue(EntryBatch model) =>
            model.AnsweredAt.ToUniversalTime().ToString(Languages.ISO8601DateFormat);

        public override string GSISortKey(EntryBatch model) =>
            $"{ModelPrefix}#{SortKeyValue(model)}";

        public override EntryBatch ToDynamoItem(EntryBatch model)
        {
            return ToDynamoItem("META", model);
        }

        public override EntryBatch ToDynamoItem(string parentId, EntryBatch model)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = parentId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = $"{GSIPartitionKey(model)}#{Survey.Prefix}#{model.SurveyId}";
            model.GSISK = GSISortKey(model);
            model.GSIPK2 = $"{GSIPartitionKey(model)}#{ProjectPatient.Prefix}#{model.PatientId}";
            model.GSIPK3 = $"{ModelPrefix}#META#{((!ValidatedSurveys.GetValidatedSurveyIdsAll().Contains(model.SurveyId)) ? "CUSTOM#" : "")}{Project.Prefix}#{model.ProjectId}#{Strategy.Prefix}#{model.StrategyId}#{Survey.Prefix}#{model.SurveyId}";
            model.GSISK3 = $"{ModelPrefix}#{model.AnsweredAt.ToUniversalTime().ToString(Languages.ISO8601DateFormat)}";

            // TODO TEMP FIX
            if (!ValidatedSurveys.GetValidatedSurveyIdsAll().Contains(model.SurveyId))
                model.GSIPK = $"CUSTOM#{model.GSIPK}";

            model.GSISK2 = GSISortKey(model);
            return model;
        }

        public EntryBatchRepository(IAmazonDynamoDB client) : base(client)
        {
        }

        public async Task<List<EntryBatch>> ReadBetween(SurveyAccess access)
        {
            var readAsPatient = !string.IsNullOrEmpty(access.PatientId);
            var searchRequiredComposition =
                $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}"
                + (readAsPatient
                    ? $"#{ProjectPatient.Prefix}#{access.PatientId}"
                    : $"#{Survey.Prefix}#{access.SurveyId}");
            var indexName = readAsPatient ? EntryBatch.GlobalSecondaryIndex2 : CrudPropModel.GlobalSecondaryIndex;
            var results = await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = indexName
                }
            ).GetRemainingAsync();
            if (readAsPatient || results.Any()) return results;
            searchRequiredComposition =
                $"CUSTOM#{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}"
                + 
                $"#{Survey.Prefix}#{access.SurveyId}";
                
            return await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = indexName
                }
            ).GetRemainingAsync();

        }
        public async Task<List<EntryBatch>> ReadBetweenSurveysByPatientId(SurveyAccess access)
        {
            var searchRequiredComposition =
                $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}" + $"#{ProjectPatient.Prefix}#{access.PatientId}";
            return await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = EntryBatch.GlobalSecondaryIndex2
                }
            ).GetRemainingAsync();
        }
        public async Task<List<EntryBatch>> ReadBetweenCustomSurveysBySurveyId(SurveyAccess access)
        {
            var searchRequiredComposition =
                $"CUSTOM#{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}" + $"#{Survey.Prefix}#{access.SurveyId}";

            return await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = CrudPropModel.GlobalSecondaryIndex
                }
            ).GetRemainingAsync();
        }

        public async Task<List<EntryBatch>> ReadBetweenSurveyByStrategyIdAndSurveyId(SurveyAccess access)
        {
            var searchStart = access.SearchStart.Date.ToLocalTime();
            var searchEnd = access.SearchEnd.Date.ToLocalTime();
            var start = $"{ModelPrefix}#{new DateTime(searchStart.Year, searchStart.Month, searchStart.Day, 0, 0, 0, 0).ToString(Languages.ISO8601DateFormat)}";
            var end = $"{ModelPrefix}#{new DateTime(searchEnd.Year, searchEnd.Month, searchEnd.Day, 23, 59, 59, 999).ToString(Languages.ISO8601DateFormat)}";
            var searchRequiredComposition = $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}#{Strategy.Prefix}#{access.StrategyId}#{Survey.Prefix}#{access.SurveyId}";

            return await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    start,
                    end,
                },
                new DynamoDBOperationConfig
                {
                    IndexName = EntryBatch.GlobalSecondaryIndex3
                }
            ).GetRemainingAsync();
        }
        public async Task<List<EntryBatch>> ReadByTag(string projectId, string patientId, string tag)
        {
            var searchRequiredComposition = "BATCH#META#PROJECT#" + projectId + "#PATIENT#" + patientId;
            var res = await Context.QueryAsync<EntryBatch>(
                searchRequiredComposition,
                new DynamoDBOperationConfig
                {
                    IndexName = Registration.GlobalSecondaryIndex2
                }
            ).GetRemainingAsync();
            return res.Where(r=>  !r.Tags.IsNullOrEmpty() && r.Tags.Contains(tag)).ToList();
            
        }
    }


    public interface IFieldEntriesRepository: ICrudPropertyRepository<FieldEntry>
    {
        Task<List<FieldEntry>> ReadByTag(string projectId, string patientId, string tag);
        Task<List<FieldEntry>> ReadByPatient(string projectId, string patientId);
        Task<List<FieldEntry>> ReadBetweenSurveyByStrategyIdAndSurveyId(SurveyAccess access);
        Task<List<FieldEntry>> ReadBetweenCustomSurveyByStrategyIdAndSurveyId(SurveyAccess access);

        Task<List<FieldEntry>> ReadAllCustomSurveyByStrategyIdAndSurveyId(SurveyAccess access);

    }
    
    public class FieldEntriesRepository : CrudPropertyRepository<FieldEntry>, IFieldEntriesRepository
    {
        public override string ParentPrefix => EntryBatch.Prefix;
        public override string ModelPrefix => FieldEntry.Prefix;

        public override string SortKeyValue(FieldEntry model) =>
            model.AnsweredAt.ToUniversalTime().ToString(Languages.ISO8601DateFormat);

        public override string GSISortKey(FieldEntry model) =>
            $"{ModelPrefix}#{SortKeyValue(model)}";
        

        public override FieldEntry ToDynamoItem(string parentId, FieldEntry model)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = parentId;
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = GSISortKey(model);
            model.GSIPK3 = $"{ModelPrefix}#META#{((!ValidatedSurveys.GetValidatedSurveyIdsAll().Contains(model.SurveyId)) ? "CUSTOM#" : "")}{Project.Prefix}#{model.ProjectId}#{Strategy.Prefix}#{model.StrategyId}#{Survey.Prefix}#{model.SurveyId}";
            model.GSISK3 = $"{ModelPrefix}#{model.AnsweredAt.ToUniversalTime().ToString(Languages.ISO8601DateFormat)}";

            return model;
        }

        public FieldEntriesRepository(IAmazonDynamoDB client) : base(client)
        {
        }

        public async Task<List<FieldEntry>> ReadByTag(string projectId, string patientId, string tag)
        {
            return await Context.ScanAsync<FieldEntry>(
                new[]
                {
                    new ScanCondition("SK", ScanOperator.BeginsWith, "ENTRY"),
                    new ScanCondition("ProjectId", ScanOperator.Contains, projectId),
                    new ScanCondition("PatientId", ScanOperator.Equal, patientId),
                    new ScanCondition("Tags", ScanOperator.Contains, tag)
                }
            ).GetRemainingAsync();
        }
        
        public async Task<List<FieldEntry>> ReadByPatient(string projectId, string patientId)
        {
            return await Context.ScanAsync<FieldEntry>(
                new[]
                {
                    new ScanCondition("ProjectId", ScanOperator.Equal, projectId),
                    new ScanCondition("PatientId", ScanOperator.Equal, patientId)
                }
            ).GetRemainingAsync();
        }
        
        public async Task<List<FieldEntry>> ReadBetweenSurveyByStrategyIdAndSurveyId(SurveyAccess access)
        {
            var searchRequiredComposition = $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}#{Strategy.Prefix}#{access.StrategyId}#{Survey.Prefix}#{access.SurveyId}";
            return await Context.QueryAsync<FieldEntry>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = FieldEntry.GlobalSecondaryIndex3
                }
            ).GetRemainingAsync();
        }

        public async Task<List<FieldEntry>> ReadBetweenCustomSurveyByStrategyIdAndSurveyId(SurveyAccess access)
        {
            var searchRequiredComposition = $"{ModelPrefix}#META#{"CUSTOM#"}{Project.Prefix}#{access.ProjectId}#{Strategy.Prefix}#{access.StrategyId}#{Survey.Prefix}#{access.SurveyId}";
            var searchStart = access.SearchStart.Date.ToLocalTime();
            var searchEnd = access.SearchEnd.Date.ToLocalTime();

            var start = $"{ModelPrefix}#{new DateTime(searchStart.Year, searchStart.Month, searchStart.Day, 0, 0, 0, 0).ToString(Languages.ISO8601DateFormat)}";
            var end = $"{ModelPrefix}#{new DateTime(searchEnd.Year, searchEnd.Month, searchEnd.Day, 23, 59, 59, 999).ToString(Languages.ISO8601DateFormat)}";
            return await Context.QueryAsync<FieldEntry>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    start,
                    end,
                },
                new DynamoDBOperationConfig
                {
                    IndexName = FieldEntry.GlobalSecondaryIndex3
                }
            ).GetRemainingAsync();
        }

        
        public async Task<List<FieldEntry>> ReadAllCustomSurveyByStrategyIdAndSurveyId(SurveyAccess access)
        {
            var searchRequiredComposition = $"{ModelPrefix}#META#{"CUSTOM#"}{Project.Prefix}#{access.ProjectId}#{Strategy.Prefix}#{access.StrategyId}#{Survey.Prefix}#{access.SurveyId}"; 
            return await Context.QueryAsync<FieldEntry>(
                searchRequiredComposition,
                new DynamoDBOperationConfig
                {
                    IndexName = FieldEntry.GlobalSecondaryIndex3
                }
            ).GetRemainingAsync();
        }
    }   
    

    
    public interface IRegistrationRepository : ICrudRepository<Registration>
    {
        Task<List<Registration>> ReadBetween(RegistrationAccess access);
        Task<List<Registration>> ReadByTag(string projectId, string patientId, string tag);
        Task<List<Registration>> ReadByEffectId(string projectId, string effectId);
        Task<List<Registration>> ReadBeforeById(string projectId, string effectId);
        Task<List<Registration>> GetStrategyRegsByEffectIdAndTypes(RegistrationAccess access);
        Task<Registration> CreateRegisteration(Registration model, string strategyId);
    }

    public class RegistrationRepository : CrudRepository<Registration>, IRegistrationRepository
    {
        public override string ParentPrefix => Registration.Prefix;
        public override string ModelPrefix => Registration.Prefix;

        public override string GSIPartitionKey(Registration model) =>
            $"{ModelPrefix}#META#{Project.Prefix}#{model.ProjectId}";
        public override string SortKeyValue(Registration model) =>
            model.Date.ToUniversalTime().ToString(Languages.ISO8601DateFormat);
        public override string GSISortKey(Registration model) =>
            $"{ModelPrefix}#{SortKeyValue(model)}";

        public override Registration ToDynamoItem(Registration model)
        {
            return ToDynamoItem("META", model);
        }

        public Registration ToDynamoItem(string parentId, Registration model, string strategyId)
        {
            model.Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id;
            model.Id = model.Id.IdUrlEncode();
            model.ParentId = parentId;
            model.CreatedAt = DateTime.Now;
            model.UpdatedAt = DateTime.Now;
            model.PK = PartitionKeyByParentId(parentId);
            model.SK = SortKey(model);
            model.GSIPK = GSIPartitionKey(model);
            model.GSISK = $"REGISTRATION#{model.Date.ToUniversalTime().ToString(Languages.ISO8601DateFormat)}";
            model.GSIPK2 = $"{GSIPartitionKey(model)}#{ProjectPatient.Prefix}#{model.PatientId}";
            model.GSISK2 = $"REGISTRATION#{model.Date.ToUniversalTime().ToString(Languages.ISO8601DateFormat)}";
            model.GSIPK3 = $"{ModelPrefix}#META#{Project.Prefix}#{model.ProjectId}#{Strategy.Prefix}#{strategyId}#TYPE#{model.Type.ToUpper()}#{StrategyEffect.Prefix}#{model.EffectId}";
            model.GSISK3 = GSISortKey(model);

            return model;
        }

        public RegistrationRepository(IAmazonDynamoDB client) : base(client)
        {
        }
        
        public async Task<List<Registration>> ReadBetween(RegistrationAccess access)
        {
            var readAsPatient = !string.IsNullOrEmpty(access.PatientId);
            
            var searchRequiredComposition =
                $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}"
                + (readAsPatient ? $"#{ProjectPatient.Prefix}#{access.PatientId}" : "");

            var indexName = readAsPatient ? Registration.GlobalSecondaryIndex2 : CrudPropModel.GlobalSecondaryIndex;

            return await Context.QueryAsync<Registration>(
                searchRequiredComposition,
                QueryOperator.Between,
                new object[]
                {
                    $"{ModelPrefix}#{access.SearchStart.ToString(Languages.ISO8601DateFormat)}",
                    $"{ModelPrefix}#{access.SearchEnd.ToString(Languages.ISO8601DateFormat)}",
                },
                new DynamoDBOperationConfig
                {
                    IndexName = indexName
                }
            ).GetRemainingAsync();
        }
        public async Task<List<Registration>> ReadByTag(string projectId, string patientId, string tag)
        {

            var searchRequiredComposition = "REGISTRATION#META#PROJECT#" +projectId + "#PATIENT#" + patientId;
            var res = await Context.QueryAsync<Registration>(
                searchRequiredComposition,
                new DynamoDBOperationConfig
                {
                    IndexName = Registration.GlobalSecondaryIndex2
                }
            ).GetRemainingAsync();
            return res.ToList();
        }
        public async Task<List<Registration>> ReadByEffectId(string projectId, string effectId)
        {
            var allRegistrations = await ReadAllRegistrations(projectId);
            var registrations = allRegistrations.Where(t => t.EffectId == effectId);
            return registrations.ToList();
        }
        public async Task<List<Registration>> ReadBeforeById(string projectId, string effectId)
        {
            var allRegistrations = await ReadAllRegistrations(projectId);
            var registrations = allRegistrations.Where(t => t.Before != null && t.Before.Id == effectId);
            return registrations.ToList();
        }
        private async Task<List<Registration>> ReadAllRegistrations(string projectId)
        {
            var searchRequiredComposition = "REGISTRATION#META#PROJECT#" + projectId;
            var res = await Context.QueryAsync<Registration>(
                searchRequiredComposition,
                new DynamoDBOperationConfig
                {
                    IndexName = CrudPropModel.GlobalSecondaryIndex
                }
            ).GetRemainingAsync();
            return res.ToList();
        }
        public async Task<List<Registration>> GetStrategyRegsByEffectIdAndTypes(RegistrationAccess access)
        {
            var searchRequiredComposition = $"{ModelPrefix}#META#{Project.Prefix}#{access.ProjectId}#{Strategy.Prefix}#{access.StrategyId}#TYPE#{access.Type}#{StrategyEffect.Prefix}#{access.EffectId}";
            var searchEnd = access.SearchEnd.Date.ToLocalTime();
            var date = $"{ModelPrefix}#{new DateTime(searchEnd.Year, searchEnd.Month, searchEnd.Day, 23, 59, 59, 999).ToString(Languages.ISO8601DateFormat)}";

            return await Context.QueryAsync<Registration>(
              searchRequiredComposition,
              QueryOperator.LessThanOrEqual,
              new object[]
              {
                date
              },
              new DynamoDBOperationConfig
              {
                  IndexName = Registration.GlobalSecondaryIndex3
              }
          ).GetRemainingAsync();
        }
        public async Task<Registration> CreateRegisteration(Registration model, string strategyId)
        {
            model = ToDynamoItem(model.ParentId, model, strategyId);
            await Context.SaveAsync(model);
            return model;
        }
    }
}