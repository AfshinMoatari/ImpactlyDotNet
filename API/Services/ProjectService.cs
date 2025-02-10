using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using API.Extensions;
using API.Models.Projects;
using API.Models.Reports;
using API.Models.Strategy;
using API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Nest;

namespace API.Services
{
    public interface IProjectService
    {
        /// <summary>
        /// Language of a project by the Id.
        /// </summary>
        /// <returns>Language of a project.</returns>
        public Task<string> GetProjectLanguageById(string projectId);
        /// <summary>
        /// Check and return the supported culture from the HttpRequest for the project by projectId
        /// </summary>
        /// <returns>Return the supported culture for the project by projectId.</returns>
        public Task<string> GetProjectCultureByProjectId(HttpRequest request, string projectId);
        /// <summary>
        /// Get all the project reports by projectId.
        /// </summary>
        /// <returns>Project reports.</returns>
        public Task<IEnumerable<Report>> GetProjectReportsByProjectId(string projectId);
        /// <summary>
        /// Update the project reports if they contain the deleted effects for an strategy.
        /// </summary>
        /// <returns>Updated project reports.</returns>
        public Task<List<Report>> UpdateProjectReportsByProjectIdAndStrategyIdAndEffectId(string projectId, string strategyId, List<string> effectsId);
        /// <summary>
        /// Update the project reports if they contain the deleted strategyId.
        /// </summary>
        /// <returns>Updated project reports.</returns>
        public Task<IEnumerable<Report>> UpdateProjectReportsByProjectIdAndStrategyId(string projectId, string strategyId);
        /// <summary>
        /// Delete the project and cascade the deletion.
        /// </summary>
        /// <returns>Returns the deleted project object if successful.</returns>
        public Task DeleteProjectByIdAndUserId(string projectId, string userId);
    }

    /// <summary>
    /// Service class for managing projects.
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectContext _projectContext;
        private readonly ILanguageAttributeExtension _languageAttributeExtension;
        private readonly IStrategyService _strategyService;
        private readonly IStrategyContext _strategyContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectService"/> class.
        /// </summary>
        /// <param name="analytics">The _project.</param>
        public ProjectService(IProjectContext projectContext, ILanguageAttributeExtension languageAttributeExtension, IStrategyService strategyService, IStrategyContext strategyContext)
        {
            _projectContext = projectContext;
            _languageAttributeExtension = languageAttributeExtension;
            _strategyService = strategyService;
            _strategyContext = strategyContext;
        }

        public async Task<string> GetProjectLanguageById(string projectId)
        {
            try
            {
                var project = await _projectContext.Projects.Read(projectId);

                var language = Languages.Default;
                if (project.TextLanguage.IsNullOrEmpty())
                {
                    language = project.TextLanguage;
                }
                return language;
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with fetching the language of the projectId of {projectId}", e);
            }
        }

        public async Task<string> GetProjectCultureByProjectId(HttpRequest request, string projectId)
        {
            try
            {
                var usercultures = request.Headers["Accept-Language"].ToString();
                var firstCulture = usercultures.Split(',').FirstOrDefault();
                var requestedculture = string.IsNullOrEmpty(firstCulture) ? "en-US" : firstCulture;

                var projectLanguage = await GetProjectLanguageById(projectId);
                var supportedCultures = _languageAttributeExtension.GetLanguageSpecificCultures(typeof(Languages), projectLanguage);

                if (!supportedCultures.IsNullOrEmpty() && supportedCultures.Any(x => x.Contains(requestedculture)))
                {
                    return requestedculture;
                }
                else
                {
                    return null;
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with checking the cultural infor for the project id of {projectId}", e);
            }
        }

        public async Task<IEnumerable<Report>> GetProjectReportsByProjectId(string projectId)
        {
            try
            {
                return await _projectContext.Reports.ReadAll(projectId);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with getting all the projetc reports for projectId {projectId}", e);
            }
        }

        public async Task<List<Report>> UpdateProjectReportsByProjectIdAndStrategyIdAndEffectId(string projectId, string strategyId, List<string> effectsId)
        {
            var result = new List<Report>();
            foreach (var effectId in effectsId)
            {
                var reports = await GetProjectReportsByProjectId(projectId);
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
                        result.AddRange(reports);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException($"Something went wrong with updating the projetc reports for projectId {projectId} and srtategyId {strategyId} and effectId: {effectId}", e);
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<Report>> UpdateProjectReportsByProjectIdAndStrategyId(string projectId, string strategyId)
        {
            var reports = await GetProjectReportsByProjectId(projectId);
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

            return reports;
        }

        public async Task DeleteProjectByIdAndUserId(string projectId, string userId)
        {
            //TODO: This need a refactoring everything need to move to project context

            //Delete project
            await _projectContext.DeleteProject(projectId);

            //Delete project users
            await _projectContext.DeleteProjectUsers(projectId);

            //Delete user project
            await _projectContext.DeleteProjectUser(projectId, userId);

            //Delete Patients
            await _projectContext.DeleteProjectPatients(projectId);

            //Delete Strategies
            var strategies = await _strategyService.GetStrategies(projectId);
            if (!strategies.IsNullOrEmpty())
            {
                foreach (var strategy in strategies)
                {
                    await _strategyService.DeleteStrategy(projectId, strategy.Id);
                }
            }

            //Delete Surveys
            var surveys = await _strategyContext.ReadProjectSurveys(projectId);
            if (!surveys.IsNullOrEmpty())
            {
                foreach (var survey in surveys)
                {
                    await _strategyContext.DeleteProjectSurvey(projectId, survey.Id);
                }
            }

            //Delete Tags
            var tags = await _projectContext.Tags.ReadAll(projectId);
            if (!tags.IsNullOrEmpty())
            {
                await _projectContext.Tags.DeleteBatch(tags);
            }

            //Delete Reports
            var reports = await _projectContext.Reports.ReadAll(projectId);
            if (!reports.IsNullOrEmpty())
            {
                foreach (var report in reports)
                {
                    await _projectContext.Reports.Delete(projectId, report.Id);
                }
            }
        }
    }
}