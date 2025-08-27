using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UserService.Core.Services;

namespace UserService.Api.Authorization
{
    public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(options);

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        {
            return _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // ANY policy name will be treated as a permission requirement
            // No need to pre-register policies!
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult(policy.Build())!;
        }
    }
}