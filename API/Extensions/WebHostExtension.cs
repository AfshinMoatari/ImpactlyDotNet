using System.Threading.Tasks;
using API.Dynamo;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace API.Extensions
{
    public static class WebHostExtension
    {
        public static async Task CreateTables(this IWebHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            await DynamoTableHelper.CreateTables(scope.ServiceProvider);
        }
        
        public static async Task SeedTables(this IWebHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            await DynamoSeedHelper.SeedTables(scope.ServiceProvider);
        }
        
        public static async Task CreateIndices(this IWebHost webHost)
        {
            using var scope = webHost.Services.CreateScope();
            //await ElasticIndexHelper.CreateIndices(scope.ServiceProvider);
        }
    }
}