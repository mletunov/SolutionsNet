using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core
{
    public static partial class Functional
    {
        public static T Lock<T>(Func<T> acquire, TimeSpan delay, CancellationToken token) where T: class
        {
            do
            {
                var result = acquire();
                if (result != null)
                    return result;

            } while (!token.WaitHandle.WaitOne(delay));

            token.ThrowIfCancellationRequested();
            throw new InvalidOperationException();
        }
        public static Action Delayed(Action action, TimeSpan delay, CancellationToken token)
        {
            var manual = new ManualResetEventSlim(false);

            Task.Factory.StartNew(() =>
            {
                if (!token.WaitHandle.WaitOne(delay))
                    manual.Set();
            }, token);

            return Delayed(action, manual.WaitHandle, token);
        }
        public static Action Delayed(Action action, WaitHandle delayHandle, CancellationToken token)
        {            
            return () => Task.Factory.StartNew(() =>
            {
                while (!delayHandle.WaitOne(TimeSpan.FromSeconds(1)))
                {
                    if (token.IsCancellationRequested)
                        return;
                }

                action();
            }, token);
        }

        public static Tuple<Action, CancellationToken> ToDelayed(Tuple<Action, CancellationToken> tuple, TimeSpan delay)
        {
            return new Tuple<Action, CancellationToken>(Delayed(tuple.Item1, delay, tuple.Item2), tuple.Item2);
        }
        public static Tuple<Action, CancellationToken> ToDelayed(Tuple<Action, CancellationToken> tuple, WaitHandle delayHandle)
        {
            return new Tuple<Action, CancellationToken>(Delayed(tuple.Item1, delayHandle, tuple.Item2), tuple.Item2);
        }
        public static Tuple<Action, CancellationToken> ToSequence(Tuple<Action, CancellationToken> tuple, Action<CancellationToken> action)
        {
            return new Tuple<Action, CancellationToken>(() =>
            {
                tuple.Item1();
                action(tuple.Item2);
            }, tuple.Item2);
        }

        public static T WithLock<T>(Func<CancellationToken, T> func, Func<CancellationToken, Tuple<Action, CancellationToken>> lockFunc, CancellationToken token)
        {
            return Using(func, lockFunc(token));
        }
        public static T WithLock<T>(Func<CancellationToken, T> func, Func<CancellationToken, Tuple<Action, CancellationToken>> lockFunc, TimeSpan delay, CancellationToken token)
        {
            return Using(func, ToDelayed(lockFunc(token), delay));
        }
        public static T WithLock<T>(Func<CancellationToken, T> func, Func<CancellationToken, Tuple<Action, CancellationToken>> lockFunc, WaitHandle delayHandle, CancellationToken token)
        {
            return Using(func, ToDelayed(lockFunc(token), delayHandle));
        }

        public static void WithLock(Action<CancellationToken> action, Func<CancellationToken, Tuple<Action, CancellationToken>> lockFunc, CancellationToken token)
        {
            WithLock(ToFunc<CancellationToken, Boolean>(action), lockFunc, token);
        }
        public static void WithLock(Action<CancellationToken> action, Func<CancellationToken, Tuple<Action, CancellationToken>> lockFunc, TimeSpan delay, CancellationToken token)
        {
            WithLock(ToFunc<CancellationToken, Boolean>(action), lockFunc, delay, token);
        }            
    } 
}
