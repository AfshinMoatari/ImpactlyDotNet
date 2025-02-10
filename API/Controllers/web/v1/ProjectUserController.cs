using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2.Model;
using API.Constants;
using API.Handlers;
using API.Models;
using API.Models.Auth;
using API.Models.Projects;
using API.Repositories;
using API.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    [Authorize(PolicyNames.ProjectAccess)]
    [ApiController]
    [Route("api/web/v1/projects/{projectId}/users")]
    public class ProjectUsersController : BaseController
    {
        private readonly IProjectContext _projectContext;
        private readonly IUserContext _userContext;
        private readonly IEmailHandler _emailHandler;
        private readonly IAuthHandler _authHandler;

        public ProjectUsersController(IProjectContext projectContext, IEmailHandler emailHandler,
            IAuthHandler authHandler, IUserContext userContext)
        {
            _projectContext = projectContext;
            _emailHandler = emailHandler;
            _authHandler = authHandler;
            _userContext = userContext;
        }

        [HttpPost, Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectUser>> Create([FromRoute] string projectId,
            [FromBody] ProjectUser request)
        {
            var message = GetMessage();
            var user = await _userContext.Users.ReadOrCreate(
                new AuthUser
                {
                    Id = request.Id,
                    Email = request.Email.Trim().ToLower(),
                    PhoneNumber = request.PhoneNumber,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                });

            var existingUser = await _projectContext.ReadProjectUser(projectId, user.Id);
            if (existingUser != null)
            {
                // if user with this email is already added to the project return error
                if (existingUser.Email == request.Email) return ErrorResponse.UserEmailAlreadyExists();
                // if user with this phonenumber is already added to the project return error
                if (existingUser.PhoneNumber == request.PhoneNumber) return ErrorResponse.UserPhoneAlreadyExists();
            }

            // if not already registered send confirm email
            if (!user.PrivacyPolicy)
            {
                var response = await SendInviteToken(user);
                if (response.HttpStatusCode != HttpStatusCode.OK)
                    return ErrorResponse.BadRequest(message.ErrorEmailFailed());
            }

            request.Id = user.Id;
            request.Email = user.Email;
            var projectUser = await _projectContext.CreateProjectUser(projectId, request);
            return Ok(projectUser);
        }

        [HttpPost("{userId}/invite")]
        public async Task<ActionResult> ResendInvite([FromRoute] string userId)
        {
            var message = GetMessage();
            var existingUser = await _userContext.Users.Read(userId);
            var response = await SendInviteToken(existingUser);
            return response.HttpStatusCode != HttpStatusCode.OK
                ? ErrorResponse.BadRequest(message.ErrorEmailFailed())
                : Ok();
        }

        [HttpPut("{userId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectUser>> UpdateUser([FromRoute] string projectId, [FromRoute] string userId,
            [FromBody] ProjectUser user)
        {
            var message = GetMessage();
            if(user.Id != userId) return ErrorResponse.BadRequest(message.ErrorDifferentUser());
            
            var existingUser = await _userContext.Users.Read(userId);
            if (existingUser == null) return ErrorResponse.BadRequest(message.ErrorNotFoundUser());

            return Ok(await _projectContext.UpdateProjectUser(projectId, user));
        }

        [HttpGet, Authorize(Permissions.Users.Read)]
        public async Task<ActionResult<IEnumerable<ProjectUser>>> ReadAll([FromRoute] string projectId)
        {
            var users = await _projectContext.ReadAllProjectUsers(projectId);
            return Ok(users);
        }

        [HttpGet("{userId}"), Authorize(Permissions.Users.Read)]
        public async Task<ActionResult<ProjectUser>> Read([FromRoute] string projectId, [FromRoute] string userId)
        {
            var response = await _projectContext.ReadProjectUser(projectId, userId);
            if (response == null) return NotFound();
            return Ok(response);
        }

        [HttpDelete("{userId}"), Authorize(Permissions.Users.Write)]
        public async Task<ActionResult<ProjectUser>> Delete([FromRoute] string projectId, [FromRoute] string userId)
        {
            var message = GetMessage();
            var existingUser = await _projectContext.ReadProjectUser(projectId, userId);
            if (existingUser == null) return ErrorResponse.BadRequest(message.ErrorNotFoundUser());
            
            await _projectContext.DeleteProjectUser(projectId, userId);
            return Ok();
        }

        private async Task<SendEmailResponse> SendInviteToken(AuthUser user)
        {
            var message = GetMessage();
            var token = _authHandler.CreateRegisterToken(user);
            var url = $"{EnvironmentMode.ClientHostForEmail}/register?token={token}";

            var response =
                await _emailHandler.SendEmail(
                    $"Impactly: {CurrentProjectName()}",
                    user.Email,
                    message.WelcomeTo("Impactly"),
                    new WelcomeAuthUserEmail(message)
                    {
                        Title = message.WelcomeTo("Impactly"),
                        ProjectName = CurrentProjectName(),
                        UserName = user.Name,
                        InviterName = CurrentUserName(),
                        DownloadUrl = url
                    }, "");

            return response;
        }
    }
}