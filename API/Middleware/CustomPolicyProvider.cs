using System;
using System.Threading.Tasks;
using API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace API.Middleware
{
    public class CustomPolicyProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
        public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

        public CustomPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName == PolicyNames.ProjectAccess)
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new ProjectRequirement(policyName));
                return Task.FromResult(policy.Build());
            }

            if (!policyName.StartsWith(PolicyNames.Permissions, StringComparison.OrdinalIgnoreCase))
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PermissionRequirement(policyName));
                return Task.FromResult(policy.Build());
            }
        }
    }
}