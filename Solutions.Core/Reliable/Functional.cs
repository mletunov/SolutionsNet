using System;
using System.Threading;

namespace Solutions.Core
{
    public static partial class Functional
    {
        /// <summary> Re-invoke func if exception is occured </summary>
        public static T Reliable<T>(Func<T> func, Func<Exception, Boolean> retry,
            Func<Int32, Exception, TimeSpan?> delay, CancellationToken token)
        {
            var count = 0;
            TimeSpan wait;

            do
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (retry == null || !retry(ex))
                        throw;

                    var toDelay = delay != null ? delay(count++, ex) : null;
                    if (toDelay == null)
                        throw;

                    wait = toDelay.Value;
                }
            } while (!token.WaitHandle.WaitOne(wait));

            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException();
        }

        /// <summary> Re-invoke func if exception is occured during wait timeout. 
        /// TimeoutException is raised when wait timeout is elapsed </summary>
        public static T Reliable<T>(Func<T> func, Func<Exception, Boolean> retry, TimeSpan wait, CancellationToken token)
        {
            return Wait(cancel => Reliable(func, retry, (count, ex) => TimeSpan.Zero, cancel), wait, token);
        }

        /// <summary> Re-invoke func if exception is occured specified number of times </summary>
        public static T Reliable<T>(Func<T> func, Func<Exception, Boolean> retry, Int32 count, CancellationToken token)
        {
            Func<Int32, Exception, TimeSpan?> delay = (i, ex) => i <= count ? (TimeSpan?)TimeSpan.Zero : null;
            return Reliable(func, retry, delay, token);
        }
    }
}