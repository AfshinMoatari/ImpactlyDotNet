using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatchLogs;
using API.Constants;
using API.Handlers;
using API.Views;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;

namespace API.Middleware
{
    public static class RequestLogging
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLogMiddleware>();
        }
    }

    public class RequestLogMiddleware
    {
        private const string MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000:int} ms.";

        private static readonly List<string> PathBlackList = new List<string>
        {
            "/", // AWS OK check
            "/health", // AWS loadbalancer request every 10 seconds
            "/negotiate", // signalR request every 2 minutes per client connection
        };

        private readonly RequestDelegate _next;
        private readonly List<string> _supportEmails = new List<string> {"han@impactly.dk"};
        private readonly ILogger _log;

        public RequestLogMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;

            if (EnvironmentMode.IsDevelopment || EnvironmentMode.IsTest)
            {
                _log = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
                return;
            }

            var serilogConfig = configuration.GetSection("Serilog");

            int.TryParse(serilogConfig.GetSection("LogLevel").Value, out var level);
            var logLevel = typeof(LogEventLevel).IsEnumDefined(level) ? (LogEventLevel) level : LogEventLevel.Error;

            var options = new CloudWatchSinkOptions
            {
                LogGroupName = serilogConfig.GetSection("LogGroup").Value,
                TextFormatter = new JsonFormatter(),
                MinimumLogEventLevel = logLevel,
                BatchSizeLimit = 100,
                QueueSizeLimit = 10000,
                Period = TimeSpan.FromSeconds(10),
                CreateLogGroup = true,
                LogStreamNameProvider = new DefaultLogStreamProvider(),
                RetryAttempts = 5
            };

            var region = RegionEndpoint.GetBySystemName(serilogConfig.GetSection("Region").Value);
            var client = new AmazonCloudWatchLogsClient(region);

            _log = new LoggerConfiguration()
                .WriteTo.AmazonCloudWatch(options, client)
                .CreateLogger();
        }

        public async Task Invoke(HttpContext context, IEmailHandler emailHandler, IWebHostEnvironment env)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Enable request body buffering
            context.Request.EnableBuffering();
            
            // Read the request body
            string requestBody = "";
            if (context.Request.ContentLength > 0)
            {
                using (var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;  // Reset the position to allow reading again
                }
            }

            try
            {
                await _next(context);
                await LogHttpRequest(context, stopwatch, null, requestBody);
            }
            catch (Exception exception)
            {
                await LogHttpRequest(context, stopwatch, exception, requestBody);
                if (!env.IsDevelopment())
                {
                    SendExceptionNotificationEmail(emailHandler, context, exception);
                }
                throw;
            }
        }

        private async void SendExceptionNotificationEmail(IEmailHandler emailHandler, HttpContext context,
            Exception exception)
        {
            var body = $"<b>TimeStamp</b>: {DateTime.Now}<br/>" +
                       $"<b>Environment</b>: {EnvironmentMode.Environment}<br/>" +
                       $"<b>Version</b>: {EnvironmentMode.Version}<br/>" +
                       $"<b>RequestPath</b>: {context.Request.Path}<br/>" +
                       $"<b>ExceptionMessage</b>: {exception.Message}<br/>" +
                       $"<b>StackTrace</b>:<br/>{exception.StackTrace}<br/>".Replace("\n", "<br/>");

            var model = new BaseEmail(new MessageEnglish())
            {
                Message = $"<b>TimeStamp</b>: {DateTime.Now}<br/>" +
                          $"<b>Environment</b>: {EnvironmentMode.Environment}<br/>" +
                          $"<b>Version</b>: {EnvironmentMode.Version}<br/>" +
                          $"<b>RequestPath</b>: {context.Request.Path}<br/>" +
                          $"<b>ExceptionMessage</b>: {exception.Message}<br/>",
                DownloadUrl = "",
            };
            await emailHandler.SendEmail($"Impactly.API{EnvironmentMode.Environment}", _supportEmails,
                $"[ERROR] {exception.Message}", body, model, ""
                );
        }

        private async Task LogHttpRequest(HttpContext httpContext, Stopwatch stopwatch, Exception exception, string requestBody)
        {
            stopwatch.Stop();
            var request = httpContext.Request;
            var statusCode = httpContext.Response?.StatusCode;
            var elapsedTime = stopwatch.Elapsed.TotalMilliseconds;

            if (PathBlackList.Any(path => httpContext.Request.Path.Value.Equals(path)))
            {
                return;
            }

            // Add to logging context
            (await LogForHttpContext(httpContext))
                .ForContext("RequestBody", requestBody)
                .ForContext("ResponseHeaders", httpContext.Response.Headers)
                .ForContext("RequestSize", httpContext.Request.ContentLength)
                .ForContext("ResponseSize", httpContext.Response.ContentLength)
                .ForContext("MemoryUsed", GC.GetTotalMemory(false))
                .Write(
                    exception != null ? LogEventLevel.Error : LogEventLevel.Information, 
                    MessageTemplate, 
                    request.Method, 
                    request.Path, 
                    statusCode ?? 500, 
                    elapsedTime
                );
        }

        private Task<ILogger> LogForHttpContext(HttpContext context)
        {
            return Task.FromResult(_log
                    .ForContext("RequestMethod", context.Request.Method)
                    .ForContext("RequestPath", context.Request.Path)
                    .ForContext("StatusCode", context.Response?.StatusCode)
                    .ForContext("RequestProtocol", context.Request.Protocol)
                    .ForContext("RequestHost", context.Request.Host)
                    .ForContext("RequestIp", context.Connection.RemoteIpAddress?.ToString())
                    .ForContext("RequestContentType", context.Request.ContentType)
                    .ForContext("RequestClientOS", context.Request.Headers["x-os"])
                    .ForContext("RequestClientVersion", context.Request.Headers["x-version"])
                    .ForContext("RequestClientDeviceId", context.Request.Headers["x-device-id"])
                    .ForContext("RequestAuthorizationUID", context.User.FindFirst(JwtClaimNames.UserId)?.Value)
                    .ForContext("RequestAuthorizationPID", context.User.FindFirst(JwtClaimNames.ProjectId)?.Value)
                    .ForContext("RequestHeaders", context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))
                    .ForContext("RequestQueryString", context.Request.QueryString.Value)
                    );
        }


    }
}