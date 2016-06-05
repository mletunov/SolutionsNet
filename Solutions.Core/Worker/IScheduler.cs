using System;
using System.Threading;

namespace Solutions.Core.Worker
{
    public interface IScheduler
    {
        /// <summary> Returns next action and delay time </summary>
        Tuple<TimeSpan, Action<CancellationToken>> Next();
    }
}