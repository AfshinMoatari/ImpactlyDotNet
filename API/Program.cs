using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using API.Constants;
using API.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (EnvironmentMode.IsDevelopment)
            {
                Console.WriteLine("Remember to start local env");
                Console.WriteLine("docker-compose -f docker-compose-local-aws.yml up --detach & dynamodb-admin");
            }

            AWSConfigsDynamoDB.Context.TableNamePrefix = EnvironmentMode.TablePrefix;

            var host = CreateHostBuilder(args).Build();
            var webHost = host.Services.GetService<IWebHost>();

            if (EnvironmentMode.IsDevelopment || EnvironmentMode.IsTest)
            {
                Console.WriteLine("Initialising Elastic Indices");
                //await webHost.CreateIndices();

                Console.WriteLine("Initialising Dynamo Tables");
                //await webHost.CreateTables();

                Console.WriteLine("Initialising Dynamo Seeds");
                //await webHost.SeedTables();
            }

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration((hostingContext, config) =>
                        {
                            config.AddEnvironmentVariables("JWTConfig_");
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseKestrel(options => 
                        {
                            options.Limits.MinRequestBodyDataRate = new MinDataRate(
                                bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(10)
                            );
                            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(60);
                            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                        });
                })
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));
    }
}