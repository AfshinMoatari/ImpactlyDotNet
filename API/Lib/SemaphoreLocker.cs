using System;
using System.Threading;
using System.Threading.Tasks;

namespace API.Lib
{
    public class SemaphoreLocker
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async Task LockAsync(Func<Task> worker)
        {
            try
            {
                await _semaphore.WaitAsync();
                await worker();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}