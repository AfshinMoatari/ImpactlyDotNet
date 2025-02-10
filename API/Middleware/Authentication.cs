using System;
using System.Threading.Tasks;
using API.Models.Config;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Middleware
{
    public static class Authentication
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services,
            JWTConfig jwtConfig)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // TODO this token can be made more secure with simple additional arguments
                    // options.Audience = jwtConfig.Audience;
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = jwtConfig.SigningKey,
                        ValidateLifetime = true,
                        RequireExpirationTime = false,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken)) context.Token = accessToken;
                            return Task.CompletedTask;
                        }
                    };
                });
            return services;
        }
    }
}