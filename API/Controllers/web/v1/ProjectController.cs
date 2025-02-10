using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using API.Constants;
using API.Models;
using API.Models.Projects;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers.web.v1
{
    [ApiController]
    [Route("api/web/v1/projects")]
    public class ProjectController : BaseController
    {
        private readonly IProjectService _projectService;
        private readonly IProjectContext _projectContext;
        private readonly IUserContext _userContext;

        public ProjectController(IProjectService projectService, IProjectContext projectContext, IUserContext userContext)
        {
            _projectService = projectService;
            _projectContext = projectContext;
            _userContext = userContext;
        }

        [HttpPost, Authorize(Permissions.Admin.All)]
        public async Task<ActionResult<Project>> Create([FromBody] Project request)
        {
            var project = await _projectContext.Projects.Create(request);
            var currentUserId = CurrentUserId();
            var user = await _userContext.Users.Read(currentUserId);

            await _projectContext.CreateProjectUser(project.Id, new ProjectUser
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                RoleId = "administrator"
            });

            return Ok(project);
        }

        [HttpPost("{projectId}/theme"), Authorize(Permissions.Admin.All)]
        public async Task<ActionResult<Project>> UpdateTheme([FromRoute] string projectId, [FromBody] Project project)
        {
            var message = GetMessage();
            var existingProject = await _projectContext.Projects.Read(projectId);
            if (existingProject == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }

            project.Id = projectId;
            var res = await _projectContext.UpdateProject(project);

            var theme = res.Theme;
           
            var reports = await _projectContext.Reports.ReadAll(projectId);
            if (reports == null || theme == null) return Ok(res);
            try
            {
                foreach (var report in reports.Where(x => x.ModuleConfigs != null))
                {
                    foreach (var moduleConfig in report.ModuleConfigs.ToList())
                    {
                        moduleConfig.Colors = theme;
                    }

                    await _projectContext.Reports.UpdateValue(projectId, report.Id,
                        report1 => report1.ModuleConfigs = report.ModuleConfigs);
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Something went wrong with updating the project reports themes for projectId {projectId}", e);
            }

            return Ok(res);
        }

        [HttpPut("{projectId}"), Authorize(Permissions.Admin.All)]
        public async Task<ActionResult<Project>> Update([FromRoute] string projectId, [FromBody] Project project)
        {
            var message = GetMessage();
            var existingProject = await _projectContext.Projects.Read(projectId);
            if (existingProject == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }

            project.Id = projectId;
            return Ok(await _projectContext.UpdateProject(project));
        }

        [HttpGet, Authorize(Permissions.Admin.All)]
        public async Task<ActionResult<IEnumerable<Project>>> ReadAll()
        {
            var message = GetMessage();
            var response = await _projectContext.Projects.ReadAll();
            if (response == null) return ErrorResponse.NotFound(message.ErrorNoProjects());
            return Ok(response);
        }

        [HttpGet("{projectId}"), Authorize(PolicyNames.ProjectAccess)]
        public async Task<ActionResult<Project>> Read([FromRoute] string projectId)
        {
            var message = GetMessage();
            var response = await _projectContext.Projects.Read(projectId);
            if (response == null) return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            return Ok(response);
        }

        [HttpDelete("{projectId}"), Authorize(Permissions.Admin.All)]
        public async Task<ActionResult<Project>> Delete([FromRoute] string projectId)
        {
            var message = GetMessage();
            // TODO DELETE PROJECT TREE!!
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }

            await _projectService.DeleteProjectByIdAndUserId(projectId, CurrentUserId());
            return Ok(project);
        }

        [HttpGet("{projectId}/communication")]
        public async Task<ActionResult<List<ProjectCommunication>>> GetCommunication([FromRoute] string projectId)
        {
            var message = GetMessage();
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }
            var comm = await _projectContext.Communicaitons.ReadAll(projectId);
            var commList = comm.ToList();
            if (!commList.Any())
            {
                commList.Add(new ProjectCommunication()
                {
                    MessageType = ProjectCommunication.CommunicationTypeWelcome,
                    MessageContent = message.DefaultWelcomeMessage().Replace("@Model.ProjectName", project.Name)
                        .Replace("Med venlig hilsen \n", ""),
                });
                commList.Add(new ProjectCommunication()
                {
                    MessageType = ProjectCommunication.CommunicationTypeSurvey,
                    MessageContent = message.DefaultWelcomeMessage().Replace("@Model.ProjectName", project.Name)
                        .Replace("Med venlig hilsen \n", ""),
                });
            }
            return Ok(commList);
        }


        [HttpPost("{projectId}/communication")]
        public async Task<ActionResult<Project>> UpdateCommunications(
            [FromRoute] string projectId, [FromBody] ProjectCommunicationRequest request)
        {
            var message = GetMessage();
            var project = await _projectContext.Projects.Read(projectId);
            if (project == null)
            {
                return ErrorResponse.NotFound(message.ErrorNotFoundProject());
            }

            var comm = await _projectContext.Communicaitons.ReadAll(projectId);


            var welcomeComm = comm.FirstOrDefault(i => i.MessageType == ProjectCommunication.CommunicationTypeWelcome) ??
                          new ProjectCommunication()
                          {
                              MessageType = ProjectCommunication.CommunicationTypeWelcome,
                          };
            welcomeComm.MessageContent = request.WelcomeMessage;

            if (welcomeComm.Id == null)
            {
                await _projectContext.Communicaitons.Create(projectId, welcomeComm);
            }
            else
            {
                await _projectContext.Communicaitons.Update(projectId, welcomeComm.Id,
                    u => { u.MessageContent = request.WelcomeMessage; });
            }

            var surveyComm = comm.FirstOrDefault(i => i.MessageType == ProjectCommunication.CommunicationTypeSurvey) ??
                             new ProjectCommunication()
                             {
                                 MessageType = ProjectCommunication.CommunicationTypeSurvey
                             };
            surveyComm.MessageContent = request.SurveyMessage;
            if (surveyComm.Id == null)
            {
                await _projectContext.Communicaitons.Create(projectId, surveyComm);
            }
            else
            {
                await _projectContext.Communicaitons.Update(projectId, surveyComm.Id,
                    communication => { communication.MessageContent = request.SurveyMessage; });
            }


            project = await _projectContext.Projects.Read(projectId);
            return Ok(project);
        }
    }
}