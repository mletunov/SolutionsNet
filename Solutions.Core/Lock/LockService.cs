using System;
using System.Threading;

namespace Solutions.Core.Lock
{
    public class LockService : ILockService
    {        
        private readonly Func<String, CancellationToken, Tuple<Action, CancellationToken>> @lock;
        public LockService(Func<String, CancellationToken, Tuple<Action, CancellationToken>> @lock)
        {
            this.@lock = @lock;
        }

        public LockService(Func<String, Tuple<Action, CancellationToken>> acquire, TimeSpan delay)
            : this((key, token) => Functional.Lock(() => acquire(key), delay, token))
        {
        }

        public ILock Lock(String key, CancellationToken token)
        {
            return new LockImpl(@lock(key, token));
        }

        class LockImpl : Disposable, ILock
        {
            public LockImpl(Tuple<Action, CancellationToken> release)
                : base(release.Item1)
            {
                Token = release.Item2;
            }

            public CancellationToken Token { get; private set; }
        }
    }
}