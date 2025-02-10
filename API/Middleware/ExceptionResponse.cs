using System;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using API.Constants;
using API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace API.Middleware
{
    public static class ExceptionResponse
    {
        public static IApplicationBuilder UseExceptionResponse(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionResponseMiddleware>();
        }
    }

    public class ExceptionResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private static readonly bool IsDisabled = EnvironmentMode.IsTest;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
            {ContractResolver = new CamelCasePropertyNamesContractResolver()};

        public ExceptionResponseMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            ExceptionDispatchInfo edi = null;
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                // Get the Exception, but don't continue processing in the catch block as its bad for stack usage.
                edi = ExceptionDispatchInfo.Capture(exception);
            }

            if (edi != null)
            {
                await HandleException(context, edi);
            }
        }

        private async Task HandleException(HttpContext context, ExceptionDispatchInfo edi)
        {
            // We can't do anything if the response has already started, just abort.
            if (context.Response.HasStarted)
                edi.Throw();
            
            context.Response.StatusCode = 500;
            var exception = edi.SourceException;
            var response = ErrorResponse.Exception(exception, _env.IsDevelopment() || _env.IsStaging());
            var body = JsonConvert.SerializeObject(response, _jsonSerializerSettings);
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(body));
        }
    }
}