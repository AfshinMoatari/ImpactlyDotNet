using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using API.Models.Analytics;
using API.Models.Strategy;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Models;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/surveys")]
    public class ProjectSurveyController : BaseController
    {
        private readonly IStrategyContext _strategyContext;
        private readonly IProjectContext _projectContext;
        private readonly IAnalyticsContext _analyticsContext;
        private readonly IPatientContext _patientContext;

        public ProjectSurveyController(IStrategyContext strategyContext, IProjectContext projectContext,
            IAnalyticsContext analyticsContext, IPatientContext patientContext)
        {
            _projectContext = projectContext;
            _strategyContext = strategyContext;
            _analyticsContext = analyticsContext;
            _patientContext = patientContext;
        }

        [HttpGet]
        public async Task<ActionResult<Survey[]>> GetProjectSurveys([FromRoute] string projectId)
        {
            var surveys = (await _strategyContext.ReadProjectSurveys(projectId)).ToList();
            return Ok(surveys);
        }

        [HttpPost]
        public async Task<ActionResult<Survey>> CreateSurvey([FromRoute] string projectId,
            [FromBody] Survey survey)
        {
            survey.TextLanguage = (await _projectContext.Projects.Read(projectId)).TextLanguage ?? Languages.Default;
            var res = await _strategyContext.CreateProjectSurvey(projectId, survey);
            return Ok(res);
        }
        [HttpPut("{surveyId}")]
        public async Task<ActionResult<Survey>> UpdateSurvey([FromRoute] string projectId, [FromRoute] string surveyId, [FromBody] Survey survey)
        {
            // Client shouldn't be allow removing or adding new choices
            // need to add the new function to add new choices
            var message = GetMessage();
            var readSurvey = await _strategyContext.ReadSurvey(projectId, survey.Id);
            if (readSurvey == null) return ErrorResponse.NotFound(message.ErrorNotFoundSurvey());

            survey.TextLanguage = readSurvey.TextLanguage;
            var res = await _strategyContext.UpdateProjectSurvey(projectId, survey);
            //cascading the effects to the old answers:
            var patients = await _patientContext.ProjectPatients.ReadAll(projectId);
            var batches = new List<EntryBatch>();
            foreach (var patient in patients)
            {
                var rs = await _analyticsContext.EntryBatches.ReadBetween(new SurveyAccess
                {
                    ProjectId = projectId,
                    PatientId = patient.Id,
                    SearchStart = DateTime.MinValue,
                    SearchEnd = DateTime.MaxValue
                });
                batches.AddRange(rs);
            }
            foreach (var batch in batches)
            {
                var entries = await _analyticsContext.FieldEntries.ReadAll(batch.Id);
                var fields = survey.Fields.ToDictionary(f => f.Id);

                foreach (var entry in entries)
                {
                    if (entry?.FieldId == null) continue;
                    if (!fields.TryGetValue(entry.FieldId, out var field)) continue;
                    entry.FieldText = field.Text;
                    var choice = field.Choices.FirstOrDefault(c => c.Id == entry.ChoiceId);
                    if (choice != null)
                    {
                        entry.ChoiceText = choice.Text;
                        entry.Text = choice.Text;
                    }
                    await _analyticsContext.FieldEntries.Update(batch.Id, entry);
                }
            }

            //Go through the reports, and update the changes
            var reports = await _projectContext.Reports.ReadAll(projectId);
            foreach (var report in reports)
            {
                var moduleConfigs = report.ModuleConfigs;
                var toUpdate = false;
                foreach (var moduleConfig in moduleConfigs)
                {
                    if (moduleConfig.SurveyId == null || !string.Equals(moduleConfig.SurveyId, survey.Id)) continue;
                    var fieldId = moduleConfig.FieldId;
                    var field = survey.Fields.FirstOrDefault(f => string.Equals(f.Id, fieldId));
                    var oldField = readSurvey.Fields.FirstOrDefault(f => string.Equals(f.Id, fieldId));
                    if (oldField == null || field == null || !string.Equals(moduleConfig.Name, oldField.Text)) continue;
                    moduleConfig.Name = field.Text;
                    toUpdate = true;

                }
                if (toUpdate)
                {
                    await _projectContext.Reports.UpdateValue(projectId, report.Id,
                        u => u.ModuleConfigs = moduleConfigs);
                }
            }

            if (string.Equals(survey.Name, readSurvey.Name)) return Ok(res);

            // Find all strategies that have that survey as part of strategy
            var strategies = await _strategyContext.ReadStrategies(projectId);
            var strategiesWithSurvey =
                strategies.Where(strategy =>
                    strategy.Surveys.Find(strategySurvey =>
                        String.Equals(strategySurvey.Id, survey.Id)) != null).ToList();

            // Go through them all and update the survey name on the strategy...
            foreach (var strategy in strategiesWithSurvey)
                await _strategyContext.Strategies.UpdateValue(
                    projectId, strategy.Id, strategySurvey =>
                    {
                        strategySurvey.Surveys = strategySurvey.Surveys.Select(sur =>
                        {
                            if (!String.Equals(sur.Id, survey.Id)) return sur;

                            sur.Name = survey.Name;
                            return sur;
                        }).ToList();
                    });



            return Ok(res);
        }

        [HttpDelete("{surveyId}")]
        public async Task<ActionResult<Survey>> DeleteProjectSurvey([FromRoute] string projectId,
            [FromRoute] string surveyId)
        {
            var deletedSurvey = await _strategyContext.DeleteProjectSurvey(projectId, surveyId);
            if (deletedSurvey == null) return ErrorResponse.Conflict("Spørgeskema bliver brugt af en strategi");

            return Ok(deletedSurvey);
        }

        // [HttpDelete("{surveyId}/fields/{fieldId}")]
        // public async Task<ActionResult<Survey>> DeleteProjectSurveyField([FromRoute] string surveyId, [FromRoute] string fieldId)
        // {
        //     var surveyField = await _strategyContext.SurveyFields.Read(surveyId, fieldId);
        //     if (surveyField == null) return ErrorResponse.NotFound("Feltet kunne ikke findes");
        //     
        //     await _strategyContext.SurveyFields.Delete(surveyId, fieldId);
        //     return Ok(surveyField);
        // }
    }
}