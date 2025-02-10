using System;
using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace API.Middleware
{
    public class PolicyAuthorizationHandler : AuthorizationHandler<ProjectRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PolicyAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ProjectRequirement requirement)
        {
            if (context.User == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var routeValues = _httpContextAccessor?.HttpContext?.Request?.RouteValues;
            var routeProjectId = routeValues?["projectId"] as string ?? routeValues?["id"] as string;
            var projectId = context.User.Claims.FirstOrDefault(c => c.Type == JwtClaimNames.ProjectId)?.Value;
            if (string.IsNullOrEmpty(projectId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (routeProjectId != projectId)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class ProjectRequirement : IAuthorizationRequirement
    {
        public string ProjectId { get; }

        public ProjectRequirement(string projectId)
        {
            ProjectId = projectId;
        }
    }
}