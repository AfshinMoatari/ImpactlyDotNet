using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API.Extensions
{
    public static class CorsPolicyExtension
    {
        public static class CorsPolicies
        {
            public const string Development = nameof(Development);
            public const string Production = nameof(Production);
        }

        public static IApplicationBuilder UseEnvCors(this IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            var corsPolicyName = env.IsProduction() ? CorsPolicies.Production : CorsPolicies.Development;
            return app.UseCors(corsPolicyName);
        }

        public static IServiceCollection AddEnvCors(this IServiceCollection services)
        {
            // TODO: set urls
            // TODO: only allow from Impactly driven domains
            return services.AddCors(
                options =>
                {
                    options.AddPolicy(
                        CorsPolicies.Development,
                        c => c
                            .SetIsOriginAllowed(_ => true)
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                    );
                    options.AddPolicy(
                        CorsPolicies.Production,
                        c => c
                            .SetIsOriginAllowed(_ => true)
                            .AllowCredentials()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                    );
                }
            );
        }
    }
}