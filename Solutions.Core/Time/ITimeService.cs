using System;

namespace Solutions.Core.Time
{
    public interface ITimeService
    {
        DateTime Now { get; }
    }
}