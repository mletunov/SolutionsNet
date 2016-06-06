using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core
{
    public static partial class Functional
    {
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

        public static T Using<T>(Func<CancellationToken, T> func, Action release, CancellationToken token)
        {
            using (new Disposable(release))
            {
                return func(token);
            }
        }
        public static T Using<T>(Func<CancellationToken, T> func, Tuple<Action, CancellationToken> tuple)
        {
            return Using(func, tuple.Item1, tuple.Item2);
        }

        public static T Wait<T>(Func<CancellationToken, T> func, TimeSpan wait, CancellationToken token)
        {
            var source = new CancellationTokenSource();
            var manual = CancellationTokenSource.CreateLinkedTokenSource(new[] {token});
#if !V4_0
            var task = Task.Run(() =>
#else
            var task = Task.Factory.StartNew(() =>
#endif
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

        public static Tuple<Action, CancellationToken> Heartbeat(Action beat, TimeSpan delay)
        {
            var source = new CancellationTokenSource();
            var manual = new ManualResetEventSlim(false);

#if !V4_0
            var task = Task.Run(() => 
#else
            var task = Task.Factory.StartNew(() =>
#endif
            {
                try
                {
                    while (!manual.WaitHandle.WaitOne(delay))
                    {
                        beat();
                    }
                }
                catch
                {
                    source.Cancel();
                }
            }, source.Token);

            return new Tuple<Action, CancellationToken>(() =>
            {
                manual.Set();
                task.Wait(source.Token);
            }, source.Token);
        }
        public static T WithHeartbeat<T>(Func<CancellationToken, T> func, Action beat, TimeSpan delay,
            CancellationToken token)
        {
            var heartbeat = Heartbeat(beat, delay);
            var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(token, heartbeat.Item2).Token;

            return Using(func, heartbeat.Item1, cancelToken);
        }
    }
}