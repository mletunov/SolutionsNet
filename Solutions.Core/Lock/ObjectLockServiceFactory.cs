using System;
using System.Collections.Generic;
using System.Threading;

namespace Solutions.Core.Lock
{
    public class ObjectLockServiceFactory : ILockServiceFactory
    {
        private readonly IDictionary<String, SemaphoreSlim> lockObjects = new Dictionary<String, SemaphoreSlim>();
        
        public ILockService GetService(Int32 maxCount)
        {
            return new LockService(AcquireLock(maxCount), TimeSpan.FromSeconds(1));
        }

        private Func<String, Tuple<Action, CancellationToken>> AcquireLock(Int32 maxCount)
        {
            return key =>
            {
                if (!lockObjects.ContainsKey(key))
                    lock (lockObjects)
                        if (!lockObjects.ContainsKey(key))
                            lockObjects[key] = new SemaphoreSlim(maxCount, maxCount);

                var semaphore = lockObjects[key];
                if (!semaphore.Wait(TimeSpan.Zero))
                    return null;

                return new Tuple<Action, CancellationToken>(() => semaphore.Release(), new CancellationToken());
            };
        }
    }
}
