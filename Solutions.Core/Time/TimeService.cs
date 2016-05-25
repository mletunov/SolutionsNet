using System;

namespace Solutions.Core.Time
{
    public class TimeService : ITimeService
    {
        private readonly Func<DateTime> func;

        public static TimeService Default
        {
            get { return new TimeService(() => DateTime.Now); }
        }
        public static TimeService DefaultUtc
        {
            get { return new TimeService(() => DateTime.UtcNow); }
        }

        public TimeService(Func<DateTime> func)
        {
            this.func = func;
        }

        public DateTime Now
        {
            get { return func(); }
        }
    }
}