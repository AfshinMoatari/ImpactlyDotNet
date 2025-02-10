using System.Linq;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace API.Middleware
{
    public static class ModelValidation
    {
        public static IServiceCollection AddModelValidation(this IServiceCollection services)
        {
            return services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorMessage = context.ModelState.Values
                        .SelectMany(x => x.Errors).FirstOrDefault()?.ErrorMessage;
                    return ErrorResponse.BadRequest(errorMessage);
                };
            });
        }
    }
}