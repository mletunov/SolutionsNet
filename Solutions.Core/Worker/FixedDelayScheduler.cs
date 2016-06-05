using System;
using System.Threading;

namespace Solutions.Core.Worker
{
    public sealed class FixedDelayScheduler : IScheduler
    {
        private readonly Func<CancellationToken, Boolean> func;
        private readonly TimeSpan delay;
        public Boolean needDelay;

        public FixedDelayScheduler(Func<CancellationToken, Boolean> func, TimeSpan delay, Boolean firstDelay = false)
        {
            this.func = func;
            this.delay = delay;
            needDelay = firstDelay;
        }

        public Tuple<TimeSpan, Action<CancellationToken>> Next()
        {
            var delayNext = TimeSpan.Zero;
            if (needDelay)
            {
                delayNext = delay;
                needDelay = false;                
            }

            return new Tuple<TimeSpan, Action<CancellationToken>>(delayNext, token =>
            {
                needDelay = func(token);
            });
        }
    }
}