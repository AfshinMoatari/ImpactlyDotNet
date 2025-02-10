using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Threading.Tasks;

public class DistributedLock
{
    private readonly ILogger<DistributedLock> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly string _lockKey;
    private readonly DistributedLockOptions _options;
    private readonly string _instanceId;
    private bool _lockAcquired;

    public DistributedLock(
        ILogger<DistributedLock> logger,
        IDistributedCache distributedCache,
        string lockKey = "GlobalLock",
        DistributedLockOptions options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _lockKey = lockKey ?? throw new ArgumentNullException(nameof(lockKey));
        _options = options ?? new DistributedLockOptions();
        _instanceId = $"{Environment.MachineName}-{Guid.NewGuid()}";
    }

    public async Task<bool> AcquireLock(string lockName, TimeSpan timeout)
    {
        var attempts = 0;
        while (attempts < _options.RetryCount)
        {
            try
            {
                var existingLock = await _distributedCache.GetAsync(lockName);
                if (existingLock != null)
                {
                    _logger.LogInformation($"Lock {lockName} is already held by another instance");
                    await Task.Delay(_options.RetryDelay);
                    attempts++;
                    continue;
                }

                var lockValue = Encoding.UTF8.GetBytes(_instanceId);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = timeout
                };

                await _distributedCache.SetAsync(lockName, lockValue, options);
                
                // Double-check the lock was actually acquired by us
                var checkLock = await _distributedCache.GetAsync(lockName);
                if (checkLock != null && Encoding.UTF8.GetString(checkLock) == _instanceId)
                {
                    _lockAcquired = true;
                    _logger.LogInformation($"Lock {lockName} acquired by instance {_instanceId}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error acquiring lock {lockName}");
                await Task.Delay(_options.RetryDelay);
                attempts++;
            }
        }

        return false;
    }

    public async Task ReleaseLock(string lockName)
    {
        if (!_lockAcquired)
        {
            return;
        }

        try
        {
            var existingLock = await _distributedCache.GetAsync(lockName);
            if (existingLock != null && Encoding.UTF8.GetString(existingLock) == _instanceId)
            {
                await _distributedCache.RemoveAsync(lockName);
                _lockAcquired = false;
                _logger.LogInformation($"Lock {lockName} released by instance {_instanceId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error releasing lock {lockName}");
        }
    }
} 