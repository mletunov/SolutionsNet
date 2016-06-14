using System;

namespace Solutions.Core.Worker
{
    public class Trigger : ITrigger
    {
        private readonly Func<TimeSpan> trigger;
        public Trigger(Func<TimeSpan> trigger)
        {
            this.trigger = trigger;
        }

        public TimeSpan Next()
        {
            return trigger();
        }
    }
}