using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using API.Models.Config;
using Microsoft.Extensions.Logging;

namespace API.Repositories
{
    public interface ILockContext
    {
        Task<bool> AcquireLock(string lockId, string instanceId, System.TimeSpan duration);
        Task ReleaseLock(string lockId, string instanceId);
        Task<bool> IsLocked(string lockId);
    }

    public class LockContext : ILockContext
    {
        private readonly IAmazonDynamoDB _client;
        private readonly string _tableName;
        private readonly ILogger<LockContext> _logger;

        public LockContext(IAmazonDynamoDB client, AWSConfig config, ILogger<LockContext> logger)
        {
            _client = client;
            _tableName = config.LockTableName;
            _logger = logger;
        }

        public async Task<bool> AcquireLock(string lockId, string instanceId, System.TimeSpan duration)
        {
            try
            {
                _logger.LogInformation($"[LockContext] Attempting to acquire lock. LockId: {lockId}, InstanceId: {instanceId}, Table: {_tableName}");

                var request = new PutItemRequest
                {
                    TableName = _tableName,
                    Item = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue { S = $"LOCK#{lockId}" } },
                        { "SK", new AttributeValue { S = $"LOCK#{lockId}" } },
                        { "InstanceId", new AttributeValue { S = instanceId } },
                        { "ExpiresAt", new AttributeValue { N = DateTimeOffset.UtcNow.Add(duration).ToUnixTimeSeconds().ToString() } }
                    },
                    ConditionExpression = "attribute_not_exists(PK) OR ExpiresAt < :now",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":now", new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() } }
                    }
                };

                await _client.PutItemAsync(request);
                _logger.LogInformation($"[LockContext] Lock acquired successfully. LockId: {lockId}");
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"[LockContext] Lock acquisition failed (already locked). LockId: {lockId}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[LockContext] Error acquiring lock. LockId: {lockId}");
                throw;
            }
        }

        public async Task ReleaseLock(string lockId, string instanceId)
        {
            try
            {
                _logger.LogInformation($"[LockContext] Attempting to release lock. LockId: {lockId}, InstanceId: {instanceId}");

                var request = new DeleteItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue { S = $"LOCK#{lockId}" } },
                        { "SK", new AttributeValue { S = $"LOCK#{lockId}" } }
                    },
                    ConditionExpression = "InstanceId = :instanceId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":instanceId", new AttributeValue { S = instanceId } }
                    }
                };

                await _client.DeleteItemAsync(request);
                _logger.LogInformation($"[LockContext] Lock released successfully. LockId: {lockId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[LockContext] Error releasing lock. LockId: {lockId}");
                throw;
            }
        }

        public async Task<bool> IsLocked(string lockId)
        {
            try
            {
                _logger.LogInformation($"[LockContext] Checking lock status. LockId: {lockId}");

                var request = new GetItemRequest
                {
                    TableName = _tableName,
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "PK", new AttributeValue { S = $"LOCK#{lockId}" } },
                        { "SK", new AttributeValue { S = $"LOCK#{lockId}" } }
                    }
                };

                var response = await _client.GetItemAsync(request);
                if (response.Item == null)
                {
                    _logger.LogInformation($"[LockContext] Lock not found. LockId: {lockId}");
                    return false;
                }

                var expiresAt = long.Parse(response.Item["ExpiresAt"].N);
                var isLocked = DateTimeOffset.UtcNow.ToUnixTimeSeconds() < expiresAt;
                _logger.LogInformation($"[LockContext] Lock status checked. LockId: {lockId}, IsLocked: {isLocked}");
                return isLocked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[LockContext] Error checking lock status. LockId: {lockId}");
                throw;
            }
        }
    }
}