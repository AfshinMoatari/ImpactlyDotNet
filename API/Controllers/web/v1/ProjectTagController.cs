using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Jobs;
using API.Models;
using API.Models.Projects;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/tags")]
    public class ProjectTagController : BaseController
    {
        private readonly IProjectContext _projectContext;
        private readonly IPatientContext _patientContext;
        private readonly IAnalyticsContext _analytics;
        public ProjectTagController(IProjectContext projectContext, IPatientContext patientContext, IAnalyticsContext analyticsContext)
        {
            _projectContext = projectContext;
            _patientContext = patientContext;
            _analytics = analyticsContext;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectTag>> Create([FromRoute] string projectId, [FromBody] ProjectTag request)
        {
            var message = GetMessage();
            var tags = await _projectContext.Tags.ReadAll(projectId);
            var tag = tags.FirstOrDefault(t => string.Equals(t.Name, request.Name, StringComparison.CurrentCultureIgnoreCase));
            if (tag != null)
            {
                return ErrorResponse.Conflict(message.ErrorTagExisted()); 
            }
            var projectTag = await _projectContext.Tags.Create(projectId, request);
            return Ok(projectTag);

        }

        [HttpGet]
        public async Task<ActionResult<List<ProjectTag>>> ReadAll([FromRoute] string projectId)
        {
            var tags = await _projectContext.Tags.ReadAll(projectId);
            return Ok(tags.Where(tag => tag.DeletedAt == null));
        }

        [HttpDelete("{tagId}"), Authorize(Permissions.Project.Write)]
        public async Task<ActionResult<ProjectTag>> DeleteTag([FromRoute] string projectId, [FromRoute] string tagId)
        {
            var tag = await _projectContext.Tags.Read(projectId, tagId);
            await _projectContext.Tags.Delete(projectId, tagId);
            UpdateTagsForPatientAndAnalytics(projectId, tagId);
            UpdateTagsForReport(projectId, tagId);
            return Ok(tag);
        }

        private async Task UpdateTagsForReport(string projectId, string projectTagId)
        {
            var reports = await _projectContext.Reports.ReadAll(projectId);
            foreach (var report in reports)
            {
                foreach (var moduleConfig in report.ModuleConfigs)
                {
                    for (var i = moduleConfig.Tags.Count - 1; i >= 0; i--)
                    {
                        if (moduleConfig.Tags[i].Id == projectTagId)
                        {
                            moduleConfig.Tags.Remove(moduleConfig.Tags[i]);
                            await _projectContext.Reports.Update(projectId, report);
                        }
                    }
                }

            }
        }
        
        private async Task UpdateTagsForPatientAndAnalytics(string projectId, string tagId)
        {

            //update the patients tag:
            var patients = await _patientContext.ProjectPatients.ReadAll(projectId);
            foreach (var patient in patients)
            {
                var patientTags = await _patientContext.Tags.ReadAll(patient.Id);
                foreach (var patientTag in patientTags)
                {
                    if (patientTag.ProjectTagId != tagId) continue;
                    await _patientContext.Tags.Delete(patient.Id, patientTag.Id);
                    await _analytics.DeleteTag(projectId, patient.Id, patientTag.Name);
                }
                patientTags = await _patientContext.Tags.ReadAll(patient.Id);
                patient.Tags.Clear();
                patient.Tags.AddRange(patientTags);
                await _patientContext.ProjectPatients.Update(projectId, patient);
            }
            
        }
    }


}