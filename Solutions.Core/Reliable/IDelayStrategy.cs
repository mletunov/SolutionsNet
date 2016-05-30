using System;

namespace Solutions.Core.Reliable
{
    public interface IDelayStrategy
    {
        /// <summary> Whether should retry and how long to delay </summary>
        /// <param name="retryCount"> How many retry attempts have been made (counts from 0) </param>
        /// <param name="lastException"> occured reliable exception </param>
        /// <returns> How long to delay before next retry attempt or null if retry is not needed </returns>
        TimeSpan? ShouldDelay(Int32 retryCount, Exception lastException);
    }
}