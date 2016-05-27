using System;
using System.Threading;

namespace Solutions.Core
{
    public static class Functional
    {
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
    }
}
