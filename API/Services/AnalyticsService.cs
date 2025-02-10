using Amazon.DynamoDBv2.DataModel;
using API.Extensions;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Strategy;
using API.Models.Views.Report;
using API.Repositories;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Reports;
using static API.Constants.Permissions;

namespace API.Services
{
    public interface IAnalyticsService
    {
        /// <summary>
        /// Create a strategy registerations.
        /// </summary>
        /// <returns></returns>
        public Task<List<Registration>> CreateRegisterations(string strategyId, List<Registration> registerations);
        /// <summary>
        /// Create a strategy registeration.
        /// </summary>
        /// <returns></returns>
        public Task CreateRegisteration(string strategyId, Registration registeration);
        /// <summary>
        /// Retrieves all survey entries betwen.
        /// </summary>
        /// <returns>A list of all enteries for an access level.</returns>
        public Task<IEnumerable<EntryBatch>> GetEntryBatchesBySurveyAccess(SurveyAccess surveyAccess, List<ProjectTag> tags);
        /// <summary>
        /// Retrieves all survey fields by the entryId.
        /// </summary>
        /// <returns>A list of all survey fields for a entryId.</returns>
        public Task<IEnumerable<FieldEntry>> GetFieldEntriesByBatchId(string batchId);
        /// <summary>
        /// Retrieves all validated survey entries by strategyId and surveyId in between dates.
        /// </summary>
        /// <returns>A list of all validated survey entries in between two dates.</returns>
        public Task<IEnumerable<EntryBatch>> GetValidatedEntryBatchesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags);
        /// <summary>
        /// Retrieves all validated survey fields by strategyId and surveyId in between dates.
        /// </summary>
        /// <returns>A list of all validated survey fields in between two dates.</returns>
        public Task<IEnumerable<FieldEntry>> GetValidatedFieldEntriesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags);
        /// <summary>
        /// Retrieves all custom survey fields by strategyId and surveyId in between dates.
        /// </summary>
        /// <returns>A list of all custom survey fields in between two dates.</returns>
        public Task<IEnumerable<FieldEntry>> GetCustomFieldEntriesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags);

        public Task<IEnumerable<FieldEntry>> GetCustomFieldEntriesAllByStrategyIdAndSurveyId(SurveyAccess surveyAccess,
            List<ProjectTag> tags);

        /// <summary>
        /// Retrieves The population size of a strategy.
        /// </summary>
        /// <returns>A numeric count of total population within an strategy.</returns>
        public Task<IEnumerable<ProjectPatient>> GetPopulationByStrategyIdAndTags(string strategyId, List<ProjectTag> tags, List<string> filters);
        /// <summary>
        /// Retrieves The registerations of a strategy by effect id.
        /// </summary>
        /// <returns>List of registeration within an strategy.</returns>
        public Task<IEnumerable<Registration>> GetStrategyRegsByEffectIdAndTypes(RegistrationAccess registrationAccess, List<ProjectTag> tags);
        /// <summary>
        /// Retrieves the registerations by effectId.
        /// </summary>
        /// <returns>List of registeration for effectId.</returns>
        public Task<IEnumerable<Registration>> GetRegistrationsByEffectId(string projectId, string effectId);
        /// <summary>
        /// Retrieves the registerations by patientId.
        /// </summary>
        /// <returns>List of registeration for patientId.</returns>
        public Task<IEnumerable<Registration>> GetRegistrationsInBetweenByPatientId(string projectId, string patientId);
        /// <summary>
        /// Update the registeration analytic records by the effectId.
        /// </summary>
        /// <returns>List of updated registeration analytic records.</returns>
        public Task<IEnumerable<Registration>> UpdateRegisterations(string projectId, List<StrategyEffect> strategyEffects);
        /// <summary>
        /// Delete the registeration analytic records by the effectIds.
        /// </summary>
        /// <returns>List of deleted registeration analytic records.</returns>
        public Task<IEnumerable<Registration>> DeleteRegisterations(string projectId, List<string> effectsId);
        /// <summary>
        /// Delete all the entrybatches for collected data under an strategy and by surveyIds.
        /// </summary>
        /// <returns>List of deleted entrybatches records.</returns>
        public Task<IEnumerable<EntryBatch>> DeleteEntryBatches(string projectId, string strategyId, List<string> surveyIds);
        /// <summary>
        /// Delete all the fieldEntries for collected data under an strategy and by surveyIds.
        /// </summary>
        /// <returns>List of deleted fieldEntries records.</returns>
        public Task<IEnumerable<FieldEntry>> DeleteFieldEntries(string projectId, string strategyId, List<string> surveyIds);

        public Task<int> GetActivePatientsCount(string strategyId, List<ProjectTag> Tags, List<string> filters,
            DateTime endDate);
    }

    /// <summary>
    /// Service class for managing analytics.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsContext _analyticsContext;
        private readonly IProjectContext _projectContext;
        private readonly ITagFiltersExtension _tagFiltersExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
        /// </summary>
        /// <param name="analytics">The _analyticsContext.</param>
        /// <param name="projectContext">The _projectContext.</param>
        /// <param name="tagFiltersExtension">The _tagFiltersExtension.</param>
        public AnalyticsService(IAnalyticsContext analytics, IProjectContext projectContext, ITagFiltersExtension tagFiltersExtension)
        {
            _analyticsContext = analytics;
            _projectContext = projectContext;
            _tagFiltersExtension = tagFiltersExtension;
        }

        public async Task<List<Registration>> CreateRegisterations(string strategyId, List<Registration> registerations)
        {
            var regsBatch = _analyticsContext.Registrations.Context.CreateBatchWrite<Registration>();
            try
            {
                var results = new List<Registration>();
                foreach (var registeration in registerations)
                {
                    try
                    {
                        var reg = await _analyticsContext.Registrations.CreateRegisteration(registeration, strategyId);
                        regsBatch.AddPutItem(reg);
                        results.Add(reg);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException($"Something went wrong with registering the patientId: {registeration.PatientId} to the strategyId: {strategyId}", e);
                    }
                }

                await _analyticsContext.Registrations.Context.ExecuteBatchWriteAsync(new BatchWrite[]
                {
                    regsBatch
                });
                return results;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with registration", e);
            }
        }

        public async Task CreateRegisteration(string strategyId, Registration registeration)
        {
            try
            {
                await _analyticsContext.Registrations.CreateRegisteration(registeration, strategyId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with registering the patientId: {registeration.PatientId} to the strategyId: {strategyId}", e);
            }

        }

        public async Task<IEnumerable<EntryBatch>> GetEntryBatchesBySurveyAccess(SurveyAccess surveyAccess, List<ProjectTag> tags)
        {
            try
            {
                var entryBatches = await _analyticsContext.EntryBatches.ReadBetween(surveyAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var entryBatchesWithTags = entryBatches.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return entryBatchesWithTags;
                }
                return entryBatches;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the entry Batches", e);
            }
        }

        public async Task<IEnumerable<FieldEntry>> GetFieldEntriesByBatchId(string batchId)
        {

            try
            {
                return await _analyticsContext.FieldEntries.ReadAll(batchId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the field entries", e);
            }
        }

        public async Task<IEnumerable<EntryBatch>> GetValidatedEntryBatchesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags)
        {
            try
            {
                var entryBatches = await _analyticsContext.EntryBatches.ReadBetweenSurveyByStrategyIdAndSurveyId(surveyAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var entryBatchesWithTags = entryBatches.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return entryBatchesWithTags;
                }
                return entryBatches;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the entry Batches", e);
            }
        }

        public async Task<IEnumerable<FieldEntry>> GetValidatedFieldEntriesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags)
        {
            try
            {
                var fieldEntries = await _analyticsContext.FieldEntries.ReadBetweenSurveyByStrategyIdAndSurveyId(surveyAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var fieldEntriesWithTags = fieldEntries.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return fieldEntriesWithTags;
                }
                return fieldEntries;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the field entries", e);
            }
        }

        public async Task<IEnumerable<FieldEntry>> GetCustomFieldEntriesInBetweenByStrategyIdAndSurveyId(SurveyAccess surveyAccess, List<ProjectTag> tags)
        {
            try
            {
                var fieldEntries = await _analyticsContext.FieldEntries.ReadBetweenCustomSurveyByStrategyIdAndSurveyId(surveyAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var fieldEntriesWithTags = fieldEntries.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return fieldEntriesWithTags;
                }
                return fieldEntries;

            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the field entries", e);
            }
        }


        public async Task<IEnumerable<FieldEntry>> GetCustomFieldEntriesAllByStrategyIdAndSurveyId(
            SurveyAccess surveyAccess, List<ProjectTag> tags)
        {
            try
            {
                var fieldEntries = await _analyticsContext.FieldEntries.ReadAllCustomSurveyByStrategyIdAndSurveyId(surveyAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var fieldEntriesWithTags = fieldEntries.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return fieldEntriesWithTags;
                }
                return fieldEntries;

            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the field entries", e);
            }
        }

        public async Task<IEnumerable<ProjectPatient>> GetPopulationByStrategyIdAndTags(string strategyId, List<ProjectTag> tags, List<string> filters)
        {
            try
            {
                var strategyPatients = await _projectContext.GetAllProjectPatientsByStrategyId(strategyId);
                var projectPatients = strategyPatients.ToList();
                if (!tags.IsNullOrEmpty())
                {
                    projectPatients = strategyPatients.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags.Select(x => x.Name).ToList())).ToList();
                }
                //if (filters == null || !filters.Contains(ReportModuleConfig.FilterExcludeOnlyOneAnswer))
                //    return projectPatients;
                //return await ExcludeOnlyOneAnswer(projectPatients);
                //exclude should be in n and N calculations:
                return projectPatients;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with counting the project patients by strategyId", e);
            }
        }

        private async Task<IEnumerable<ProjectPatient>> ExcludeOnlyOneAnswer(IEnumerable<ProjectPatient> projectPatients)
        {
            var results = new List<ProjectPatient>();
            foreach (var projectPatient in projectPatients)
            {
                var answers = await _analyticsContext.EntryBatches.ReadBetween(new SurveyAccess()
                {
                    PatientId = projectPatient.Id,
                    ProjectId = projectPatient.ParentId,
                    SearchStart = DateTime.MinValue,
                    SearchEnd = DateTime.MaxValue,
                });
                if (answers is { Count: > 1 })
                {
                    results.Add(projectPatient);
                }

            }
            return results;
        }

        public async Task<IEnumerable<Registration>> GetStrategyRegsByEffectIdAndTypes(RegistrationAccess registrationAccess, List<ProjectTag> tags)
        {
            try
            {
                var strategyRegs = await _analyticsContext.Registrations.GetStrategyRegsByEffectIdAndTypes(registrationAccess);

                if (!tags.IsNullOrEmpty())
                {
                    var strategyRegsWithTags = strategyRegs.Where(r => _tagFiltersExtension.CheckTags(tags, r.Tags)).ToList();
                    return strategyRegsWithTags;
                }
                return strategyRegs;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with retrieving the regs by effectid and types", e);
            }
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsByEffectId(string projectId, string effectId)
        {
            try
            {
                return await _analyticsContext.FindRegistrationsByEffectId(projectId, effectId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with retrieving the regs by effectid: {effectId}", e);
            }
        }

        public async Task<IEnumerable<Registration>> GetRegistrationsInBetweenByPatientId(string projectId, string patientId)
        {
            try
            {
                var registrationsAccess = new RegistrationAccess
                {
                    ProjectId = projectId,
                    PatientId = patientId,
                    SearchStart = DateTime.MinValue,
                    SearchEnd = DateTime.MaxValue
                };
                return await _analyticsContext.Registrations.ReadBetween(registrationsAccess);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with retrieving the regs by patientId: {patientId} for projectId: {projectId}", e);
            }
        }

        public async Task<IEnumerable<Registration>> UpdateRegisterations(string projectId, List<StrategyEffect> strategyEffects)
        {
            var result = new List<Registration>();  
            var regsBatch = _analyticsContext.Registrations.Context.CreateBatchWrite<Registration>();
            

            foreach (var strategyEffect in strategyEffects)
            {
                var regs = await GetRegistrationsByEffectId(projectId, strategyEffect.Id);
                if (regs != null && regs.Count() > 0)
                {
                    foreach (var reg in regs)
                    {
                        reg.EffectName = strategyEffect.Name;

                        if (reg.Type.Equals("status"))
                        {
                            foreach (var editedEffect in strategyEffects)
                            {
                                if (reg.Now != null && reg.Now.Id == editedEffect.Id)
                                {
                                    reg.Now.Name = editedEffect.Name;
                                }
                                if (reg.Before != null && reg.Before.Id == editedEffect.Id)
                                {
                                    reg.Before.Name = editedEffect.Name;
                                }
                            }
                        }
                        regsBatch.AddPutItem(reg);
                        result.Add(reg);
                    }
                }
            }

            try
            {
                await _analyticsContext.Registrations.Context.ExecuteBatchWriteAsync(
                    new BatchWrite[]
                    {
                        regsBatch
                    });
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with updating the registrations: {regsBatch} for projectId: {projectId}", e);
            }
            
            return result;
        }

        public async Task<IEnumerable<Registration>> DeleteRegisterations(string projectId, List<string> effectsId)
        {
            var result = new List<Registration>();
            var regsBatch = _analyticsContext.Registrations.Context.CreateBatchWrite<Registration>();
            foreach (var effectId in effectsId)
            {
                var regs = await GetRegistrationsByEffectId(projectId, effectId);
                regsBatch.AddDeleteItems(regs);
                result.AddRange(regs);
            }

            try
            {
                await _analyticsContext.Registrations.Context.ExecuteBatchWriteAsync(
                    new BatchWrite[]
                    {
                        regsBatch
                    });
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the registrations: {regsBatch} for projectId: {projectId}", e);
            }
            return result;
        }

        public async Task<IEnumerable<EntryBatch>> DeleteEntryBatches(string projectId, string strategyId, List<string> surveyIds)
        {
            var result = new List<EntryBatch>();
            var entryBatchesBatch = _analyticsContext.EntryBatches.Context.CreateBatchWrite<EntryBatch>();

            foreach (var surveyId in surveyIds)
            {
                var surveyaccess = new SurveyAccess()
                {
                    ProjectId = projectId,
                    StrategyId = strategyId,
                    SearchEnd = DateTime.MaxValue.Date,
                    SearchStart = DateTime.MinValue.Date,
                    SurveyId = surveyId
                };
                var entries = await GetValidatedEntryBatchesInBetweenByStrategyIdAndSurveyId(surveyaccess, null);

                entryBatchesBatch.AddDeleteItems(entries);
                result.AddRange(entries);
            }

            try
            {
                await _analyticsContext.EntryBatches.Context.ExecuteBatchWriteAsync(
                    new BatchWrite[]
                    {
                        entryBatchesBatch
                    });
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the entryBatches: {entryBatchesBatch} for projectId: {projectId} and strategyid: {strategyId}", e);
            }
            return result;
        }

        public async Task<IEnumerable<FieldEntry>> DeleteFieldEntries(string projectId, string strategyId, List<string> surveyIds)
        {
            var result = new List<FieldEntry>();
            var fieldEntriesBatch = _analyticsContext.FieldEntries.Context.CreateBatchWrite<FieldEntry>();

            foreach (var surveyId in surveyIds)
            {
                var surveyaccess = new SurveyAccess()
                {
                    ProjectId = projectId,
                    StrategyId = strategyId,
                    SearchEnd = DateTime.MaxValue.Date,
                    SearchStart = DateTime.MinValue.Date,
                    SurveyId = surveyId
                };
                var fieldEntries = await GetCustomFieldEntriesInBetweenByStrategyIdAndSurveyId(surveyaccess, null);

                fieldEntriesBatch.AddDeleteItems(fieldEntries);
                result.AddRange(fieldEntries);
            }

            try
            {
                await _analyticsContext.EntryBatches.Context.ExecuteBatchWriteAsync(
                    new BatchWrite[]
                    {
                        fieldEntriesBatch
                    });
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the fieldEntries: {fieldEntriesBatch} for projectId: {projectId} and strategyid: {strategyId}", e);
            }
            return result;
        }
        
        public async Task<int> GetActivePatientsCount(string strategyId, List<ProjectTag> Tags, List<string> filters, DateTime endDate)
        {
            var patients = await GetPopulationByStrategyIdAndTags(strategyId, Tags, filters);
            return patients.Count(p => p.CreatedAt <= endDate && p.IsActive );
        }
    }
}