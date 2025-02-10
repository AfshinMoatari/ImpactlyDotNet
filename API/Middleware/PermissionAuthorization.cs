using System.Linq;
using System.Threading.Tasks;
using API.Constants;
using Microsoft.AspNetCore.Authorization;

namespace API.Middleware
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var permissionClaims = context.User.Claims.Where(claim => claim.Type == PolicyNames.Permissions);
            if (permissionClaims.Any(claim => claim.Value == requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}