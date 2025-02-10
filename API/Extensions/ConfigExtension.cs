using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class ConfigExtension
    {
        public static TConfig AddSingletonConfig<TConfig>(
            this IServiceCollection services,
            IConfiguration configuration, string section
        ) where TConfig : class
        {
            var config = configuration.GetSection(section).Get<TConfig>();
            services.AddSingleton(config);
            return config;
        }
    }
}