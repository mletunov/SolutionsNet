using System;
using System.Threading;
using System.Threading.Tasks;

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

        public static T Wait<T>(Func<CancellationToken, T> func, TimeSpan wait, CancellationToken token)
        {
            var source = new CancellationTokenSource();
            var manual = CancellationTokenSource.CreateLinkedTokenSource(new[] {token});
            var task = Task.Factory.StartNew(() =>
            {
                manual.Token.WaitHandle.WaitOne(wait);
                source.Cancel();
            }, token);

            try
            {
                var result = func(source.Token);
                manual.Cancel();
                task.Wait(token);
                return result;
            }
            catch (OperationCanceledException ex)
            {
                token.ThrowIfCancellationRequested();
                if (ex.CancellationToken == source.Token)
                    throw new TimeoutException();

                throw;
            }
        }

        public static Action ToAction<T>(Func<T> func)
        {
            return () => func();
        }
        public static Func<T> ToFunc<T>(Action action)
        {
            return () =>
            {
                action();
                return default(T);
            };
        }
        public static Func<T1, T2> ToFunc<T1, T2>(Action<T1> action)
        {
            return t1 =>
            {
                action(t1);
                return default(T2);
            };
        }
    }    
}
