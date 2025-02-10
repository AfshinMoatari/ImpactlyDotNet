using System.Collections.Generic;
using System.Threading.Tasks;
using API.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Impactly.Test.IntegrationTests.Lib
{
    [Collection("Integration Test")]
    public class LibSemaphoreLock
    {
        private readonly ITestOutputHelper _output;

        private static readonly SemaphoreLocker SemaphoreLocker = new SemaphoreLocker();
        private const int ThreadToRun = 10;

        public LibSemaphoreLock(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Fact]
        public async Task EnsureInvariant()
        {
            var totalRunningThread = 0;
            var throttleTasks = new List<Task>();

            for (var i = 1; i <= ThreadToRun; i++)
            {
                throttleTasks.Add(Task.Run(async () =>
                {
                    await SemaphoreLocker.LockAsync(async () =>
                    {
                        totalRunningThread++;
                        try
                        {
                            await Task.Delay(100);
                            Assert.False(totalRunningThread > 1);
                        }
                        finally
                        {
                            totalRunningThread--;
                        }
                    });
                }));
            }

            await Task.WhenAll(throttleTasks);
        }
    }
}