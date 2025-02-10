using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Handlers;
using API.Models;
using API.Models.Auth;
using API.Repositories;
using API.Views;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    [ApiController]
    [Route("api/web/v1/auth")]
    public class AuthController : BaseController
    {
        private readonly IUserContext _userContext;
        private readonly IAdminContext _adminContext;
        private readonly IProjectContext _projectContext;
        private readonly IAuthHandler _authHandler;
        private readonly IEmailHandler _emailHandler;

        public AuthController(
            IEmailHandler emailHandler,
            IAuthHandler authHandler,
            IUserContext userContext,
            IProjectContext projectContext,
            IAdminContext adminContext)
        {
            _authHandler = authHandler;
            _userContext = userContext;
            _projectContext = projectContext;
            _adminContext = adminContext;
            _emailHandler = emailHandler;
        }

        [HttpGet, Authorize]
        public async Task<ActionResult<SignInResponse>> SignInWithToken()
        {
            var userId = CurrentUserId();
            var user = await _userContext.Users.Read(userId);

            var authorization = await _authHandler.CreateUserAuthorisation(user);

            return Ok(new SignInResponse
            {
                Authorization = authorization,
                User = user,
            });
        }

        [HttpPost, AllowAnonymous]
        public async Task<ActionResult<SignInResponse>> SignInWithEmail([FromBody] SignInWithEmailRequest request)
        {
            var user = await _userContext.Users.ReadByEmail(request.Email);
            if (user == null)
            {
                return ErrorResponse.InvalidPasswordOrEmail();
            }

            var passwordHashB64 = _authHandler.HashUserPassword(user.Id, request.Password);
            if (passwordHashB64 != user.PasswordHashB64) return ErrorResponse.InvalidPasswordOrEmail();

            var authorization = await _authHandler.CreateUserAuthorisation(user);
            return Ok(
                new SignInResponse
                {
                    Authorization = authorization,
                    User = user,
                }
            );
        }

        [HttpPost("admins/{adminId}"), Authorize]
        public async Task<ActionResult<Authorization>> SignInWithAdmin([FromRoute] string adminId)
        {
            var message = GetMessage();
            var currentUserId = CurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return ErrorResponse.Forbidden(message.ErrorNotFoundUser());

            if (currentUserId != adminId)
                return ErrorResponse.Forbidden(message.ErrorUserNotAdmin());

            var authUser = await _userContext.Users.Read(currentUserId);
            if (authUser == null)
                return ErrorResponse.Forbidden(message.ErrorNotFoundUser());

            var admin = await _adminContext.Admins.Read(authUser.Id);
            if (admin == null)
                return ErrorResponse.Forbidden(message.ErrorUserNotAdmin());

            var authorization = _authHandler.CreateAdminUserAuthorisation(authUser);

            return Ok(authorization);
        }

        [HttpPost("projects/{projectId}"), Authorize]
        public async Task<ActionResult<ProjectAuthResponse>> SignInWithProject([FromRoute] string projectId)
        {
            var message = GetMessage();
            var currentUserId = CurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return ErrorResponse.Forbidden(message.ErrorNoUserId());

            var userProjects = (await _projectContext.ReadAllUserProjects(currentUserId)).ToList();
            var currentProjectId = CurrentProjectId();
            var requestedProjectId = string.IsNullOrEmpty(projectId) ? currentProjectId : projectId;

            if (!userProjects.Any())
                return ErrorResponse.Forbidden(message.ErrorUserNotConnectedToAny());

            if (userProjects.All(p => p.Id != requestedProjectId))
                return ErrorResponse.Forbidden(message.ErrorUserNotConnectedToProject()+" "  + projectId);

            var project = await _projectContext.Projects.Read(requestedProjectId);

            var projectUser = await _projectContext.ReadProjectUser(projectId, currentUserId);
            var authorization = await _authHandler.CreateProjectUserAuthorization(project, projectUser);

            return Ok(new ProjectAuthResponse
            {
                Project = project,
                User = projectUser,
                Authorization = authorization
            });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<Authorization>> RefreshAuthorization(
            [FromBody] RefreshAuthorizationRequest request)
        {
            var message = GetMessage();
            if (string.IsNullOrEmpty(request?.RefreshToken))
                return ErrorResponse.BadRequest(message.ErrorRefreshTokenEmpty());

            var refreshToken = request.RefreshToken;
            var accessToken = CurrentAccessToken();

            try
            {
                var authorization = await _authHandler.RefreshAccessToken(refreshToken, accessToken);
                return Ok(authorization);
            }
            catch (Exception e)
            {
                return ErrorResponse.NotFound(message.ErrorRefreshTokenEmpty());
            }
        }

        [HttpPost("register"), Authorize(Permissions.Auth.CreatePassword)]
        public async Task<ActionResult<AuthUser>> Register([FromBody] RegisterRequest request)
        {
            var message = GetMessage();
            var currentUserId = CurrentUserId();
            if (string.IsNullOrEmpty(currentUserId)) return ErrorResponse.BadRequest(message.ErrorNoUserId());

            var existingUser = await _userContext.Users.Read(currentUserId);
            if (existingUser == null)
                return ErrorResponse.UserNotFound(message.ErrorNotFoundUser());

            var newPasswordHash = _authHandler.HashUserPassword(currentUserId, request.Password);
            var user = await _userContext.Users.UpdateValue(currentUserId,
                authUser =>
                {
                    authUser.PasswordHashB64 = newPasswordHash;
                    authUser.PrivacyPolicy = request.PrivacyPolicy;
                });

            return Ok(user);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<Authorization>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var message = GetMessage();
            var user = await _userContext.Users.ReadByEmail(request.Email);
            if (user == null) return Ok();

            var authorization = _authHandler.CreateResetPasswordToken(user);
            var response = await _emailHandler.SendEmail("Impactly", user.Email, message.PasswordReset(),
                new ForgotPasswordEmail(message)
                {
                    Title = message.PasswordReset(),
                    UserName = user.Name,
                    DownloadUrl = $"{EnvironmentMode.ClientHostForEmail}/reset-password?token={authorization}",
                }, "");

            return StatusCode((int)response.HttpStatusCode);
        }

        [HttpPost("reset-password"), Authorize(Permissions.Auth.UpdatePassword)]
        public async Task<ActionResult<Authorization>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var message = GetMessage();
            var currentUserId = CurrentUserId();
            var user = await _userContext.Users.Read(currentUserId);
            if (user == null) return ErrorResponse.UserNotFound(message.ErrorNotFoundUser());

            user.PasswordHashB64 = _authHandler.HashUserPassword(currentUserId, request.Password);
            await _userContext.Users.Update(user);
            return Ok(user);
        }
    }
}