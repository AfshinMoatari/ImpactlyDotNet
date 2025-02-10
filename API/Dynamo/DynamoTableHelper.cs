using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using API.Constants;
using API.Models.Admin;
using API.Models.Analytics;
using API.Models.Auth;
using API.Models.Codes;
using API.Models.Cron;
using API.Models.Notifications;
using API.Models.Projects;
using API.Models.Strategy;
using API.Models.Logs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.Dynamo
{
    public static class DynamoTableHelper
    {
        private static readonly ProvisionedThroughput DefaultThroughput = new ProvisionedThroughput
        {
            ReadCapacityUnits = 1,
            WriteCapacityUnits = 1,
        };

        public static async Task CreateTables(IServiceProvider serviceProvider)
        {
            try
            {
                var client = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
       
                var tableTypes = new List<Type>
                {
                    typeof(AdminUser),
                    typeof(SurveyJob),
                    typeof(Project),
                    typeof(ProjectPatient),
                    typeof(AuthUser),
                    typeof(Authorization),
                    typeof(Survey),
                    typeof(ReportCode),
                    typeof(Role),
                    typeof(EntryBatch),
                    typeof(Notification),
                    typeof(Log)
                };
                await Task.WhenAll(tableTypes.Select(async type =>
                {
                    var request = TableRequestFromType(type);
                    await EnsureTable(client, request);
                }));
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Failed to ensure tables");
                throw;
            }
        }

        private static CreateTableRequest TableRequestFromType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var properties = typeInfo.GetProperties();
            var tableName = EnvironmentMode.TablePrefix +
                            typeInfo.GetCustomAttribute<DynamoDBTableAttribute>()?.TableName;
            var hashKey = properties.First(p => p.GetCustomAttribute<DynamoDBHashKeyAttribute>() != null).Name;
            var rangeKey = properties.FirstOrDefault(p => p.GetCustomAttribute<DynamoDBRangeKeyAttribute>() != null)
                ?.Name;
            var tags = new List<Tag>
            {
                new Tag() {Key = "AppId", Value = "Impactly"},
                new Tag() {Key = "Env", Value = EnvironmentMode.Environment},
            };

            var secondaryIndexes = properties.Select(p =>
            {
                var indexName = p.GetCustomAttribute<DynamoDBGlobalSecondaryIndexHashKeyAttribute>()?.IndexNames
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(indexName))
                {
                    return null;
                }

                var pk = properties.FirstOrDefault(p =>
                {
                    var attribute = p.GetCustomAttribute<DynamoDBGlobalSecondaryIndexHashKeyAttribute>();
                    return attribute != null && attribute.IndexNames.Contains(indexName);
                });
                var sk = properties.FirstOrDefault(p =>
                {
                    var attribute = p.GetCustomAttribute<DynamoDBGlobalSecondaryIndexRangeKeyAttribute>();
                    return attribute != null && attribute.IndexNames.Contains(indexName);
                });

                if (pk == null)
                {
                    return null;
                }

                return new GlobalSecondaryIndex
                {
                    IndexName = indexName,
                    ProvisionedThroughput = DefaultThroughput,
                    Projection = new Projection {ProjectionType = ProjectionType.ALL},
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement(pk.Name, KeyType.HASH),
                        new KeySchemaElement(sk?.Name, KeyType.RANGE),
                    }.Where(key => key.AttributeName != null).ToList(),
                };
            }).Where(index => index != null).ToList();

            return new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = DefaultThroughput,
                Tags = tags,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement(hashKey, KeyType.HASH),
                    new KeySchemaElement(rangeKey, KeyType.RANGE),
                }.Where(key => key.AttributeName != null).ToList(),
                AttributeDefinitions = secondaryIndexes
                    .SelectMany(index => index.KeySchema.Select(key => key.AttributeName))
                    .Concat(new List<string> {hashKey, rangeKey})
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .Select(name => new AttributeDefinition {AttributeName = name, AttributeType = "S"})
                    .ToList(),

                GlobalSecondaryIndexes = secondaryIndexes,
            };
        }


        private static async Task EnsureTable(IAmazonDynamoDB client, CreateTableRequest request)
        {
            try
            {
                Table.LoadTable(client, request.TableName);
            }
            catch (Exception)
            {
                try
                {
                    var createTableRequest = request;
                    await client.CreateTableAsync(createTableRequest);
                    Console.WriteLine($"Table: {request.TableName} {TableStatus.CREATING}");
                    await WaitUntilTableReady(client, request.TableName);
                    Console.WriteLine($"Table: {request.TableName} {TableStatus.ACTIVE}");
                }
                catch (ResourceInUseException)
                {
                }
            }
        }

        private static async Task WaitUntilTableReady(IAmazonDynamoDB client, string tableName)
        {
            string status = null;
            // Let us wait until table is created. Call DescribeTable.
            do
            {
                Thread.Sleep(5000); // Wait 5 seconds.
                try
                {
                    var description = new DescribeTableRequest {TableName = tableName};
                    var result = await client.DescribeTableAsync(description);
                    Console.WriteLine($"Table: {result.Table.TableName}, status: {result.Table.TableStatus}");
                    status = result.Table.TableStatus;
                }
                catch (ResourceNotFoundException)
                {
                    // DescribeTable is eventually consistent. So you might
                    // get resource not found. So we handle the potential exception.
                }
            } while (status != "ACTIVE");
        }
    }
}