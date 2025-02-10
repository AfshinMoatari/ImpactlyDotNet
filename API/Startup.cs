using Amazon.S3;
using Amazon.SimpleEmailV2;
using Amazon.SimpleNotificationService;
using API.Common;
using API.Extensions;
using API.Handlers;
using API.Helpers;
using API.Lib;
using API.Mapping;
using API.Middleware;
using API.Models.Config;
using API.Operators;
using API.Repositories;
using API.Services;
using API.Services.External;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // config
            var awsConfig = services.AddSingletonConfig<AWSConfig>(Configuration, "AWSConfig");
            var jwtConfig = services.AddSingletonConfig<JWTConfig>(Configuration, "JWTConfig");

            // mc
            services.AddHealthChecks();
            services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.AddAuthentication(jwtConfig);

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddEnvCors();
            services.AddModelValidation();
            services.AddPolicyAuthorization();
            services.AddSwagger();

            //localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // aws
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddDynamo(awsConfig);
            //services.AddElastic(awsConfig);
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonSimpleEmailServiceV2>();
            services.AddAWSService<IAmazonSimpleNotificationService>();

            // contexts
            services.AddScoped<IAdminContext, AdminContext>();
            services.AddScoped<IProjectContext, ProjectContext>();
            services.AddScoped<IAnalyticsContext, AnalyticsContext>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPatientContext, PatientContext>();
            services.AddScoped<IStrategyContext, StrategyContext>();
            services.AddScoped<ICodeContext, CodeContext>();
            services.AddScoped<ICronContext, CronContext>();
            services.AddScoped<IRoleContext, RoleContext>();
            services.AddScoped<ILockContext, LockContext>();
            services.AddSingleton<INotificationContext, NotificationContext>();
            services.AddScoped<IAnalyticsReportContext, AnalyticsReportContext>();
            services.AddScoped<ILogContext, LogContext>();

            //helpers
            services.AddScoped<IS3Helper, S3Helper>();

            // handlers
            services.AddScoped<ILogHandler, LogHandler>();
            services.AddScoped<IAuthHandler, AuthHandler>();
            services.AddScoped<IEmailHandler, EmailHandler>();
            services.AddScoped<ISMSHandler, SMSHandler>();
            services.AddScoped<ISurveyHandler, SurveyHandler>();
            services.AddScoped<IImageHandler, ImageHandler>();
            services.AddScoped<IExcelHandler, ExportExcelHandler>();
            services.AddScoped<IAnonymityHandler, AnonymityHandler>();
            services.AddScoped<ITimeMachineHandler, TimeMachineHandler>();

            // extensions
            services.AddScoped<ITagFiltersExtension, TagFiltersExtension>();
            services.AddScoped<ILanguageAttributeExtension, LanguageAttributeExtension>();

            // cron jobs
            services.AddRecurring();

            // Razor
            services.AddRazorPages();
            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationFormats.Clear();
                options.ViewLocationFormats.Add("/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");

                // Add diagnostic logging
                var environment = services.BuildServiceProvider().GetService<IWebHostEnvironment>();
                var contentRootPath = environment.ContentRootPath;
                Console.WriteLine($"Content Root Path: {contentRootPath}");
                Console.WriteLine("View Locations:");
                foreach (var location in options.ViewLocationFormats)
                {
                    Console.WriteLine($"- {Path.Combine(contentRootPath, location)}");
                }
            });

            //services
            services.AddSingleton<NotificationService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            services.AddScoped<ISurveyService, SurveyService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IStrategyService, StrategyService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IJobsService, JobsService>();
            services.AddScoped<ISendoutService, SendoutService>();
            services.AddScoped<IAnalyticsReportService, AnalyticsReportService>();

            //external services
            services.AddHttpClient<IAnalyticsAPIService, AnalyticsAPIService>();
            services.AddHttpClient<IGeneratorAPIService, GeneratorAPIService>();

            //Operations
            services.AddScoped<IFrequencyOperatorContext, FrequentOperationContext>();
            services.AddScoped<IPeriodicOperationContext, PeriodicOperationContext>();
            services.AddScoped<IJobsOperatorContext, JobsOperatorContext>();

            //Add Validators by scanning the Assembly
            services.AddValidatorsFromAssemblyContaining<IAssemblyMarker>();
            services.AddFluentValidationAutoValidation();

            //Mapping
            services.AddMappings();

            //cultureInfos
            services.Configure<RequestLocalizationOptions>(
              options =>
              {
                  var supportedCultures = new List<CultureInfo>
                  {
                            new CultureInfo("da-DK"),
                            new CultureInfo("en-US"),
                            new CultureInfo("en-GB")
                  };

                  options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                  options.SupportedCultures = supportedCultures;
                  options.SupportedUICultures = supportedCultures;
              });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwaggerWithUi();

            if (!env.IsDevelopment()) app.UseHttpsRedirection();

            app.UseExceptionResponse();

            app.UseRequestLogging();

            app.UseResponseCompression();

            app.UseEnvCors(env);

            var localizeOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizeOptions.Value);

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }
    }
}