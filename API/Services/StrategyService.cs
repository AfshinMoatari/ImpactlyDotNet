using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using API.Handlers;
using API.Models.Analytics;
using API.Models.Strategy;
using API.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public interface IStrategyService
    {
        /// <summary>
        /// Get the project strategies.
        /// </summary>
        /// <returns>Project strategies.</returns>
        public Task<List<Strategy>> GetStrategies(string projectId);
        /// <summary>
        /// Get the project strategy.
        /// </summary>
        /// <returns>Strategy.</returns>
        public Task<Strategy> GetStrategy(string projectId, string strategyId);
        /// <summary>
        /// Create a new strategy
        /// </summary>
        /// <returns>A newly created strategy.</returns>
        public Task<Strategy> CreateStrategy(string projectId, Strategy strategy);
        /// <summary>
        /// Delete a strategy.
        /// </summary>
        /// <returns>Sucessfully removed strategy.</returns>
        public Task<Strategy> DeleteStrategy(string projectId, string strategyId);
        /// <summary>
        /// Update an strategy name.
        /// </summary>
        /// <returns>Sucessfully updated strategy.</returns>
        public Task<Strategy> UpdateStrategy(string projectId, string strategyId, Strategy updatedStrategy);
        /// <summary>
        /// Remove patients from strategy.
        /// </summary>
        /// <returns>A list of sucessfully removed patients from the strategy.</returns>
        public Task<List<StrategyPatient>> DeleteStrategyPatients(List<StrategyPatient> srategyPatients);
        /// <summary>
        /// Create a strategy surveys.
        /// </summary>
        /// <returns>A list of surveys that were assigned to the strategy.</returns>
        public Task<List<SurveyProperty>> CreateStrategySurveys(string projectId, string strategyId, List<SurveyProperty> surveys);
        /// <summary>
        /// Get the strategy surveys.
        /// </summary>
        /// <returns>Strategy surveys.</returns>
        public Task<List<Survey>> GetStrategySurveys(string projectId, string strategyId);
        /// <summary>
        /// Get the registeration effect status of strategy patients.
        /// </summary>
        /// <returns>Status for the available registeration category.</returns>
        public Task<List<RegistrationStatus>> GetRegisteredStatus(string projectId, string category, string strategyId);
        /// <summary>
        /// Get the strategy frequency.
        /// </summary>
        /// <returns>Strategy frequency.</returns>
        public Task<BatchSendoutFrequency> GetStrategyFrequency(string strategyId, string frequencyId);
        /// <summary>
        /// Update all the jobs and the sendout for a strategy.
        /// </summary>
        /// <returns>An updated sendout for a strategy.</returns>
        public Task<BatchSendoutFrequency> UpdateBatchSendout(string projectId, BatchSendoutFrequency strategyFrequency);
        /// <summary>
        /// Delete a selected sendout for a strategy.
        /// </summary>
        /// <returns>Deleted sendout</returns>
        public Task<BatchSendoutFrequency> DeleteBatchSendout(string strategyId, string frequencyId);
        /// <summary>
        /// Retrieves all effects by the strategy Id.
        /// </summary>
        /// <returns>A list of all effects for a strategy.</returns>
        public Task<List<StrategyEffect>> GetStrategyEffectsByStrategyId(string strategyId);
        /// <summary>
        /// Retrieves an effect by the strategyId and effectId.
        /// </summary>
        /// <returns>A effect for a strategy.</returns>
        public Task<StrategyEffect> GetStrategyEffectByStrategyIdAndEffectId(string strategyId, string effectId);
        /// <summary>
        /// Create a registeration effects.
        /// </summary>
        /// <returns>A list of newly created effects.</returns>
        public Task<List<StrategyEffect>> CreateEffects(string strategyId, List<StrategyEffect> effects);
        /// <summary>
        /// Update a list of effects for an strategyId.
        /// </summary>
        /// <returns>List of updated effects</returns>
        public Task<List<StrategyEffect>> UpdateEffects(string projectId, string strategyId, List<StrategyEffect> strategyEffects);
        /// <summary>
        /// Delete a list of effects for an strategyId.
        /// </summary>
        /// <returns>List of deleted effects</returns>
        public Task<List<StrategyEffect>> DeleteEffects(string projectId, string strategyId, List<string> effectsId);
    }

    /// <summary>
    /// Service class for managing strategies.
    /// </summary>
    public class StrategyService : IStrategyService
    {
        private readonly IStrategyContext _strategyContext;
        private readonly IPatientService _patientService;
        private readonly IJobsService _jobsService;
        private readonly IAnonymityHandler _anonymityHandler;
        private readonly IAnalyticsService _analyticsService;
        private readonly ISendoutService _sendoutService;
        //private readonly IProjectService _projectService;
        private readonly IProjectContext _projectContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyService"/> class.
        /// </summary>
        /// <param name="strategyContext">The _strategyContext.</param>
        /// <param name="patientService">The _patientService.</param>
        /// <param name="jobsService">The _jobsService.</param>
        /// <param name="anonymityHandler">The _anonymityHandler.</param>
        /// <param name="analyticsService">The _analyticsService.</param>
        /// <param name="sendoutService">The _sendoutService.</param>
        /// <param name="projectService">The _projectService.</param>
        public StrategyService(
            IStrategyContext strategyContext,
            IPatientService patientService,
            IJobsService jobsService,
            IAnonymityHandler anonymityHandler,
            IAnalyticsService analyticsService,
            ISendoutService sendoutService,
            //IProjectService projectService,
            IProjectContext projectContext
        )
        {
            _strategyContext = strategyContext;
            _patientService = patientService;
            _jobsService = jobsService;
            _anonymityHandler = anonymityHandler;
            _analyticsService = analyticsService;
            _sendoutService = sendoutService;
            //_projectService = projectService;
            _projectContext = projectContext;
        }

        public async Task<List<Strategy>> GetStrategies(string projectId)
        {
            try
            {
                return await _strategyContext.ReadStrategies(projectId);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not get the strategies for projectId: {projectId}", e);
            }
        }

        public async Task<Strategy> GetStrategy(string projectId, string strategyId)
        {
            try
            {
                return await _strategyContext.ReadStrategy(projectId, strategyId);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not get the strategy: {strategyId} for projectId: {projectId}", e);
            }
        }

        public async Task<Strategy> CreateStrategy(string projectId, Strategy strategy)
        {
            strategy.Id = Guid.NewGuid().ToString();
            strategy.Effects ??= new List<StrategyEffect>();
            strategy.Frequencies ??= new List<BatchSendoutFrequency>();
            strategy.Surveys ??= new List<SurveyProperty>();
            strategy.Patients ??= new List<StrategyPatient>();

            try
            {
                // assign the passed patients to the strategy if any
                if (!strategy.Patients.IsNullOrEmpty())
                {
                    await _patientService.CreateStrategyPatients(strategy.Patients.ToList(), projectId, strategy.Id, strategy.Name);
                }

                // add the frequency sendouts if any
                if (strategy.Frequencies.IsNullOrEmpty())
                    return await _strategyContext.CreateStrategy(projectId, strategy);
                
                foreach (var strategyFrequency in strategy.Frequencies)
                {
                    // create the strategy frequency
                    if (strategyFrequency.ParentId is null) continue;
                    var batchStrategyFreq = await _sendoutService.CreateStrategyFrequency(strategyFrequency, "Frequent");
                    // create the jobs
                    await _jobsService.CreateJobs(batchStrategyFreq, projectId, "Frequent");

                }

                // create the strategy
                return await _strategyContext.CreateStrategy(projectId, strategy);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not create the new strategy: {strategy.Id}", e);
            }
        }

        public async Task<Strategy> DeleteStrategy(string projectId, string strategyId)
        {
            var strategy = await GetStrategy(projectId, strategyId);
            if (strategy == null)
                new Exception($"Can not remove the strategy as it does not exist with strategyId: {strategyId} on projectId {projectId}");

            // delete the strategy effects
            await DeleteEffects(projectId, strategyId, strategy.Effects.Select(x => x.Id).ToList());

            //delete the strategy survey analytics
            await _analyticsService.DeleteEntryBatches(projectId, strategyId, strategy.Surveys.Select(x => x.Id).ToList());
            await _analyticsService.DeleteFieldEntries(projectId, strategyId, strategy.Surveys.Select(x => x.Id).ToList());

            // delete the strategy sendouts
            foreach (var frequencyId in strategy.Frequencies.Select(x => x.Id).ToList())
            {
                await DeleteBatchSendout(strategyId, frequencyId);
            }

            // delete the strategy patients
            await DeleteStrategyPatients(strategy.Patients.ToList());

            // delete the strategy from project patients
            await _patientService.DeleteStrategyPatients(projectId, strategy.Patients.ToList());

            // delete the strategy reports from project
            //TODO: temp solution to prevent circular dependency
            //await _projectService.UpdateProjectReportsByProjectIdAndStrategyId(projectId, strategy.Id);
            var reports = await _projectContext.Reports.ReadAll(projectId);
            if (reports != null)
            {
                try
                {
                    foreach (var report in reports.Where(x => x.ModuleConfigs != null)
                                    .Where(y => y.ModuleConfigs.Any(z => z.StrategyId == strategyId)))
                    {
                        foreach (var moduleConfig in report.ModuleConfigs.ToList())
                        {
                            if (moduleConfig.StrategyId == strategyId)
                            {
                                var layoutId = moduleConfig.Layout.I;
                                var targetLayout = report.Layouts.FirstOrDefault().Value.Where(x => x.I == layoutId)
                                    .FirstOrDefault();
                                if (targetLayout != null)
                                {
                                    report.Layouts.FirstOrDefault().Value.Remove(targetLayout);
                                }

                                report.ModuleConfigs.Remove(moduleConfig);
                            }
                        }
                    }
                    await _projectContext.Reports.UpdateBatch(reports);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Something went wrong with updating the projetc reports for projectId {projectId} and srtategyId {strategyId}", e);
                }
            }



            // delete the strategy
            await _strategyContext.Strategies.Delete(strategy.ParentId, strategy.Id);

            return strategy;
        }

        public async Task<Strategy> UpdateStrategy(string projectId, string strategyId, Strategy updatedStrategy)
        {
            var existingStrategy = await GetStrategy(projectId, strategyId);
            if (existingStrategy == null)
                new Exception($"Can not remove the strategy as it does not exist with strategyId: {strategyId} on projectId {projectId}");

            // update strategyPatiens with the new strategy name
            await _patientService.UpdateStrategyPatients(projectId, existingStrategy.Patients.ToList(), updatedStrategy.Name);
            try
            {
                existingStrategy.Name = updatedStrategy.Name;
                return await _strategyContext.Strategies.Update(existingStrategy.ParentId, existingStrategy);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not add updated the strategy Name for strategyId {strategyId}", e);
            }
        }

        public async Task<List<StrategyPatient>> DeleteStrategyPatients(List<StrategyPatient> srategyPatients)
        {
            var patientBatch = _strategyContext.StrategyPatients.Context.CreateBatchWrite<StrategyPatient>();
            patientBatch.AddDeleteItems(srategyPatients);

            try
            {
               await _strategyContext.Strategies.Context.ExecuteBatchWriteAsync(
               new BatchWrite[]
               {
                    patientBatch
               });
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not remove the strategy patients {srategyPatients}", e);
            }
            return srategyPatients;

        }

        public async Task<List<SurveyProperty>> CreateStrategySurveys(string projectId, string strategyId, List<SurveyProperty> surveys)
        {
            var strategy = await GetStrategy(projectId, strategyId);
            try
            {
                strategy.Surveys = surveys;
                await _strategyContext.Strategies.Update(strategy.ParentId, strategy);
                return surveys;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not add the surveys to strategyId {strategyId}", e);
            }

        }

        public async Task<List<Survey>> GetStrategySurveys(string projectId, string strategyId)
        {
            try
            {
                return await _strategyContext.GetAllStrategySurveysByStrategyId(projectId, strategyId);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not get the surveys for the strategyId: {strategyId} and the projectId: {projectId}", e);
            }
        }

        public async Task<List<RegistrationStatus>> GetRegisteredStatus(string projectId, string category, string strategyId)
        {
            var registrationsStatus = new List<RegistrationStatus>();
            var strategyPatients = await _patientService.GetStrategyPatients(strategyId);
            var annonymousStrategyPatients = _anonymityHandler.HidePatients(strategyPatients);

            if (annonymousStrategyPatients != null)
            {
                foreach (var strategyPatient in annonymousStrategyPatients)
                {
                    var patientRegistrations = await _analyticsService.GetRegistrationsInBetweenByPatientId(projectId, strategyPatient.Id);
                    var registrationStatus = new RegistrationStatus(strategyPatient.Id, projectId, strategyPatient.Name, strategyPatient.Tags, category);

                    if (!patientRegistrations.IsNullOrEmpty())
                    {
                        var effectRegistrations = patientRegistrations.ToList().FindAll(x => x.EffectName == category || x.Category == category).ToList().OrderBy(d => d.CreatedAt);

                        if (effectRegistrations.Count() >= 1)
                        {
                            var regStatus = "";
                            if (effectRegistrations.FirstOrDefault().Type == "status")
                            {
                                regStatus = string.Join(" -> ", effectRegistrations.Select(x => x.EffectName));
                            }
                            else if (effectRegistrations.FirstOrDefault().Type == "numeric")
                            {
                                regStatus = string.Join(" -> ", effectRegistrations.Select(x => x.Value));
                            }

                            registrationStatus = new RegistrationStatus(
                                strategyPatient.Id,
                                projectId,
                                strategyPatient.Name,
                                effectRegistrations.FirstOrDefault().Type,
                                true,
                                regStatus,
                                effectRegistrations.LastOrDefault().Date,
                                effectRegistrations.LastOrDefault().Note,
                                effectRegistrations.LastOrDefault().EffectId,
                                effectRegistrations.LastOrDefault().Category,
                                strategyPatient.Tags,
                                effectRegistrations.Select(x => x.Now).LastOrDefault()
                            );
                        }
                    }
                    registrationsStatus.Add(registrationStatus);
                }
            }
            return registrationsStatus;
        }

        public async Task<BatchSendoutFrequency> GetStrategyFrequency(string strategyId, string frequencyId)
        {
            try
            {
                return await _strategyContext.BatchFrequencies.Read(strategyId, frequencyId);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not get the strategy frequency for the strategyId: {strategyId} and the frequencyId: {frequencyId}", e);
            }
        }

        public async Task<BatchSendoutFrequency> UpdateBatchSendout(string projectId, BatchSendoutFrequency batchSendoutFrequency)
        {
            var updatedFrq = new BatchSendoutFrequency();
            try
            {
                updatedFrq = await _strategyContext.BatchFrequencies.Update(batchSendoutFrequency.ParentId, batchSendoutFrequency);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not update the strategy sendout frequency with id of : {batchSendoutFrequency.Id} and the projectId: {projectId}", e);
            }

            if (updatedFrq != null)
            {
                var jobs = await _jobsService.ReadAllJobs(updatedFrq.ParentId);

                if (jobs != null)
                {
                    await _jobsService.DeleteJobs(jobs, updatedFrq.Id);
                    await _jobsService.CreateJobs(updatedFrq, projectId, "Frequent");
                }
            }

            return updatedFrq;
        }

        public async Task<BatchSendoutFrequency> DeleteBatchSendout(string strategyId, string frequencyId)
        {
            var frequencyBatch = _strategyContext.BatchFrequencies.Context.CreateBatchWrite<BatchSendoutFrequency>();
            var existingfrequency = await GetStrategyFrequency(strategyId, frequencyId);

            if (existingfrequency != null)
            {
                var jobs = await _jobsService.ReadAllJobs(existingfrequency.ParentId);

                if (jobs != null)
                {
                    await _jobsService.DeleteJobs(jobs, existingfrequency.Id);
                }

                frequencyBatch.AddDeleteItem(existingfrequency);
            }

            try
            {
                await _strategyContext.Strategies.Context.ExecuteBatchWriteAsync(
                    new BatchWrite[]
                    {
                        frequencyBatch
                    });
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not delete the sendout with frequencyId : {frequencyId} and the strategyId: {strategyId}", e);
            }

            return existingfrequency;
        }

        public async Task<List<StrategyEffect>> GetStrategyEffectsByStrategyId(string strategyId)
        {
            try
            {
                var strategyEffects = await _strategyContext.Effects.ReadAll(strategyId);
                return strategyEffects.ToList();
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Something went wrong with fetching the effects for the strategy", e);
            }
        }

        public async Task<StrategyEffect> GetStrategyEffectByStrategyIdAndEffectId(string strategyId, string effectId)
        {
            try
            {
                return await _strategyContext.Effects.Read(strategyId, effectId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with fetching the effect for the strategyId {strategyId} and effectId {effectId}", e);
            }
        }

        public async Task<List<StrategyEffect>> CreateEffects(string strategyId, List<StrategyEffect> effects)
        {
            var results = new List<StrategyEffect>();
            foreach (var effect in effects)
            {
                try
                {
                    var result = await _strategyContext.Effects.Create(strategyId, effect);
                    results.Add(result);
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Could not create the effectid: {effect.Id} for strategyId: {strategyId}", e);
                }
            }
            return results;
        }

        public async Task<List<StrategyEffect>> UpdateEffects(string projectId, string strategyId, List<StrategyEffect> strategyEffects)
        {
            foreach (var strategyEffect in strategyEffects)
            {
                try
                {
                    await _strategyContext.Effects.Update(strategyId, strategyEffect);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Something went wrong with updating the frequency registrationId: {strategyEffect.Id} for strategyId: {strategyId} and projectId: {projectId}", e);
                }
            }

            var effectBatch = _strategyContext.BatchFrequencies.Context.CreateBatchWrite<StrategyEffect>();
            effectBatch.AddPutItems(strategyEffects);

            await _analyticsService.UpdateRegisterations(projectId, strategyEffects);

            return strategyEffects;
        }

        public async Task<List<StrategyEffect>> DeleteEffects(string projectId, string strategyId, List<string> effectsId)
        {
            var result = new List<StrategyEffect>();
            var effectsBatch = _strategyContext.Effects.Context.CreateBatchWrite<StrategyEffect>();

            foreach (var effectId in effectsId)
            {
                var effect = await GetStrategyEffectByStrategyIdAndEffectId(strategyId, effectId);
                effectsBatch.AddDeleteItem(effect);
                result.Add(effect);
            }

            try
            {
                await _strategyContext.Strategies.Context.ExecuteBatchWriteAsync(
                  new BatchWrite[]
                  {
                        effectsBatch
                  });
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with deleting the strategy effectIds: {effectsId} for strategyId: {strategyId} and projectId: {projectId}", e);
            }

            await _analyticsService.DeleteRegisterations(projectId, effectsId);


            //TODO: temp solution to prevent circular dependency 
            //await _projectService.UpdateProjectReportsByProjectIdAndStrategyIdAndEffectId(projectId, strategyId, effectsId);
            foreach (var effectId in effectsId)
            {
                var reports = await _projectContext.Reports.ReadAll(projectId);
                if (reports != null)
                {
                    try
                    {
                        foreach (var report in reports.Where(x => x.ModuleConfigs != null).Where(y => y.ModuleConfigs.Any(z => z.StrategyId == strategyId)))
                        {
                            foreach (var moduleConfig in report.ModuleConfigs.ToList())
                            {
                                if (moduleConfig.StrategyId == strategyId && moduleConfig.EffectId == effectId)
                                {
                                    var layoutId = moduleConfig.Layout.I;
                                    var targetLayout = report.Layouts.FirstOrDefault().Value.Where(x => x.I == layoutId)
                                        .FirstOrDefault();
                                    if (targetLayout != null)
                                    {
                                        report.Layouts.FirstOrDefault().Value.Remove(targetLayout);
                                    }

                                    report.ModuleConfigs.Remove(moduleConfig);
                                }
                            }
                        }
                        await _projectContext.Reports.UpdateBatch(reports);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException($"Something went wrong with updating the projetc reports for projectId {projectId} and srtategyId {strategyId} and effectId: {effectId}", e);
                    }
                }
            }




            return result;
        }

    }
}