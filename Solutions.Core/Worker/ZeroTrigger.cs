using System;

namespace Solutions.Core.Worker
{
    public class ZeroTrigger : ITrigger
    {
        private readonly TimeSpan delay;
        private Int32 counter;

        public ZeroTrigger(TimeSpan delay)
        {
            this.delay = delay;
        }

        public TimeSpan Next()
        {
            return counter++ == 0 ? TimeSpan.Zero : delay;
        }
    }
}