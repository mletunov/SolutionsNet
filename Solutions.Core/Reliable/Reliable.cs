using System;
using System.Threading;

namespace Solutions.Core.Reliable
{
    public class Reliable : IReliable
    {
        private readonly Func<Exception, Boolean> retry;
        private readonly Func<Int32, Exception, TimeSpan?> delay;
        
        public Reliable(Func<Exception, Boolean> retry, Func<Int32, Exception, TimeSpan?> delay)
        {
            this.retry = retry;
            this.delay = delay;
        }

        public Reliable(IRetryStrategy retry, Func<Int32, Exception, TimeSpan?> delay)
            : this(ToFunc(retry), delay)
        {
        }

        public Reliable(Func<Exception, Boolean> retry, IDelayStrategy delay,
            Action<Int32, Exception> retryHandler = null)
            : this(retry, ToFunc(delay, retryHandler))
        {
        }

        public Reliable(IRetryStrategy retry, IDelayStrategy delay,
            Action<Int32, Exception> retryHandler = null)
            : this(ToFunc(retry), ToFunc(delay, retryHandler))
        {
        }

        public T Execute<T>(Func<T> func, CancellationToken? token)
        {
            return Functional.Reliable(func, retry, delay, token ?? new CancellationToken());
        }

        private static Func<Exception, Boolean> ToFunc(IRetryStrategy retry)
        {
            return retry.IsTransient;
        }
        private static Func<Int32, Exception, TimeSpan?> ToFunc(IDelayStrategy delay, Action<Int32, Exception> retryHandler)
        {
            return (count, ex) =>
            {
                if (retryHandler != null)
                    retryHandler(count, ex);

                return delay.ShouldDelay(count, ex);
            };
        }
    }
}