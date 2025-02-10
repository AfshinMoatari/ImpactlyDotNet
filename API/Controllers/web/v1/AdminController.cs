using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService.Model;
using API.Constants;
using API.Models;
using API.Models.Admin;
using API.Models.Analytics;
using API.Models.Projects;
using API.Models.Strategy;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    // TODO SEPARATE CONTROLLER
    [ApiController]
    [Route("api/web/v1/admins"), Authorize(Permissions.Admin.All)]
    public class AdminController : BaseController
    {
        private readonly IUserContext _userContext;
        private readonly IAdminContext _adminContext;
        private readonly IProjectContext _projectContext;
        private readonly IStrategyContext _strategyContext;
        private readonly IAnalyticsContext _analyticsContext;
        private readonly IPatientContext _patientContext;

        public AdminController(IUserContext userContext, IAdminContext adminContext, IProjectContext projectContext, 
            IStrategyContext strategyContext,  IAnalyticsContext analyticsContext, IPatientContext patientContext)
        {
            _userContext = userContext;
            _adminContext = adminContext;
            _projectContext = projectContext;
            _strategyContext = strategyContext;
            _analyticsContext = analyticsContext;
            _patientContext = patientContext;
        }

        [HttpPost("{userId}")]
        public async Task<ActionResult<OverviewUser>> Create([FromRoute] string userId)
        {
            var message = GetMessage();
            var existingAuthUser = await _userContext.Users.Read(userId);
            if (existingAuthUser == null)
            {
                return ErrorResponse.Forbidden(message.ErrorNotFoundUser());
            }
            
            var existingAdmin = await _adminContext.Admins.Read(existingAuthUser.Id);
            if (existingAdmin != null)
            {
                return ErrorResponse.Forbidden(message.ErrorUserNotAdmin());
            }
            await _adminContext.Admins.Create(AdminUser.FromAuthUser(existingAuthUser));
            
            var user = OverviewUser.FromAuthUser(existingAuthUser);
            var projects = await _projectContext.ReadAllUserProjects(user.Id);
            user.Projects = projects.ToList();
            user.IsAdmin = true;
            
            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<OverviewUser>>> GetUsers()
        {
            var authUsers = await _userContext.Users.ReadAll();

            var users = new List<OverviewUser>();
            foreach (var authUser in authUsers)
            {
                var user = OverviewUser.FromAuthUser(authUser);
                
                var projects = await _projectContext.ReadAllUserProjects(user.Id);
                user.Projects = projects.ToList();
                
                var existingAdmin = await _adminContext.Admins.Read(user.Id);
                if (existingAdmin != null) user.IsAdmin = true;
                
                users.Add(user);
            }

            return Ok(users);
        }

        [HttpGet("projects/{projectId}/surveys")]
        public async Task<ActionResult<List<Survey>>> GetSurveys([FromRoute] string projectId)
        {
            var surveys = await _strategyContext.ReadProjectSurveys(projectId);
            return Ok(surveys);
        }

        [HttpPost("projects/{projectId}/surveys")]
        public async Task<ActionResult<Survey>> CreateSurvey([FromRoute] string projectId,
            [FromBody] Survey survey)
        {
            var message = GetMessage();
            var proj = await _projectContext.Projects.Read(projectId);
            if (proj is null)
            {
                return NotFound(message.ErrorNotFoundProject() + " projectId=" + projectId);
            }
            survey.TextLanguage = (await _projectContext.Projects.Read(projectId)).TextLanguage??Languages.Default;
            survey = await _strategyContext.AddTemplateFields(projectId, survey);
            var res = await _strategyContext.CreateProjectSurvey(projectId, survey);
            return Ok(res);
        }

        [HttpPut("projects/{projectId}/surveys")]
        public async Task<ActionResult<Survey>> UpdateSurvey([FromRoute] string projectId, [FromBody] Survey survey)
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

        [HttpDelete("projects/{projectId}/surveys/{surveyId}")]
        public async Task<ActionResult<Survey>> DeleteSurvey([FromRoute] string projectId, [FromRoute] string surveyId)
        {
            var message = GetMessage();
            var res = await _strategyContext.DeleteProjectSurvey(projectId, surveyId);
            return res == null ? ErrorResponse.Conflict(message.ErrorPatientHasStrategy()) : Ok(res);
        }

        // TEMP METHOD TO SET IFRAME SROIs
        [HttpPut("projects/{projectId}/sroi")]
        public async Task<ActionResult<Project>> SetSroiURL([FromRoute] string projectId, [FromBody] UpdateSroiRequest sroiRequest)
        {
            var message = GetMessage();
            var existingProject = await _projectContext.Projects.Read(projectId);
            if(existingProject == null) return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            existingProject.SroiUrl = sroiRequest.Url;
            var res = await _projectContext.Projects.Update(existingProject);
            return Ok(res);
        }
        
        [HttpPost("projects/{projectId}/fieldtemplate")]
        public async Task<ActionResult<FieldTemplate>> CreateFieldTemplate([FromRoute] string projectId,
            [FromBody] FieldTemplate fieldTemplate)
        {
            fieldTemplate.TextLanguage = (await _projectContext.Projects.Read(projectId)).TextLanguage??Languages.Default;
            var res = await _strategyContext.CreateFieldTemplate(projectId, fieldTemplate);
            return Ok(res);
        }

        [HttpGet("projects/{projectId}/fieldtemplates")]
        public async Task<ActionResult<List<FieldTemplate>>> ReadAllFieldTemplates([FromRoute] string projectId)
        {
            var templates = await _strategyContext.ReadFieldTemplates(projectId);
            return templates.ToList();
        }

        [HttpGet("projects/{projectId}/fieldtemplate/{fieldTemplateId}")]
        public async Task<ActionResult<FieldTemplate>> ReadFieldTemplate([FromRoute] string projectId,
            [FromRoute] string fieldTemplateId)
        {
            return await _strategyContext.FieldTemplates.Read(projectId, fieldTemplateId);
        }
        
        [HttpDelete("projects/{projectId}/fieldtemplate/{fieldTemplateId}")]
        public async Task DeleteFieldTemplate([FromRoute] string projectId,
            [FromRoute] string fieldTemplateId)
        {
            await _strategyContext.FieldTemplates.Delete(projectId, fieldTemplateId);
        }
    }
}