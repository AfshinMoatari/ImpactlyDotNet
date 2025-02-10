using System;
using Amazon.DynamoDBv2;
using API.Models.Analytics;
using API.Models.Config;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace API.Extensions
{
    public static class DataContextsExtension
    {
        public static IServiceCollection AddDynamo(this IServiceCollection services, AWSConfig awsConfig)
        {
            if (awsConfig.LocalMode)
                return services
                    .AddSingleton<IAmazonDynamoDB>(sp =>
                        new AmazonDynamoDBClient(new AmazonDynamoDBConfig
                            {ServiceURL = awsConfig.DynamoDbUrl}));
            return services
                .AddAWSService<IAmazonDynamoDB>();
        }
        
    }
}