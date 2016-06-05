using System;
using System.Threading;

namespace Solutions.Core.Lock
{
    public interface ILockService
    {
        ILock Lock(String key, CancellationToken token);
    }

    public static class LockServiceExtensions
    {
        public static Func<CancellationToken, Tuple<Action, CancellationToken>> ToFunc(this ILockService service, String key)
        {
            return token =>
            {
                var @lock = service.Lock(key, token);
                return new Tuple<Action, CancellationToken>(@lock.Dispose, @lock.Token);
            };
        }

        public static T WithLock<T>(this ILockService service, String key, Func<CancellationToken, T> func, CancellationToken? token = null)
        {
            return Functional.WithLock(func, service.ToFunc(key), token ?? new CancellationToken());
        }
        public static T WithLock<T>(this ILockService service, String key, TimeSpan wait, Func<CancellationToken, T> func, CancellationToken? token = null)
        {
            return Functional.Wait(cancel => Functional.WithLock(func, service.ToFunc(key), cancel), wait,
                token ?? new CancellationToken());
        }
        public static T WithLock<T>(this ILockService service, String key, Func<CancellationToken, T> func, TimeSpan delay, CancellationToken? token = null)
        {
            return Functional.WithLock(func, service.ToFunc(key), delay, token ?? new CancellationToken());
        }
        public static T WithLock<T>(this ILockService service, String key, TimeSpan wait, Func<CancellationToken, T> func, TimeSpan delay, CancellationToken? token = null)
        {
            return Functional.Wait(cancel => Functional.WithLock(func, service.ToFunc(key), delay, cancel), wait,
                token ?? new CancellationToken());
        }
        public static T WithLock<T>(this ILockService service, String key, Func<CancellationToken, T> func, WaitHandle delayHandle, CancellationToken? token = null)
        {
            return Functional.WithLock(func, service.ToFunc(key), delayHandle, token ?? new CancellationToken());
        }
        public static T WithLock<T>(this ILockService service, String key, TimeSpan wait, Func<CancellationToken, T> func, WaitHandle delayHandle, CancellationToken? token = null)
        {
            return Functional.Wait(cancel => Functional.WithLock(func, service.ToFunc(key), delayHandle, cancel), wait,
                token ?? new CancellationToken());
        }

        public static void WithLock(this ILockService service, String key, Action<CancellationToken> action, CancellationToken? token = null)
        {
            service.WithLock(key, Functional.ToFunc<CancellationToken, Boolean>(action), token);
        }
        public static void WithLock(this ILockService service, String key, TimeSpan wait, Action<CancellationToken> action, CancellationToken? token = null)
        {
            service.WithLock(key, wait, Functional.ToFunc<CancellationToken, Boolean>(action), token);
        }
        public static void WithLock(this ILockService service, String key, Action<CancellationToken> action, TimeSpan delay, CancellationToken? token = null)
        {
            service.WithLock(key, Functional.ToFunc<CancellationToken, Boolean>(action), delay, token);
        }
        public static void WithLock(this ILockService service, String key, TimeSpan wait, Action<CancellationToken> action, TimeSpan delay, CancellationToken? token = null)
        {
            service.WithLock(key, wait, Functional.ToFunc<CancellationToken, Boolean>(action), delay, token);
        }
        public static void WithLock(this ILockService service, String key, Action<CancellationToken> action, WaitHandle delayHandle, CancellationToken? token = null)
        {
            service.WithLock(key, Functional.ToFunc<CancellationToken, Boolean>(action), delayHandle, token);
        }
        public static void WithLock(this ILockService service, String key, TimeSpan wait, Action<CancellationToken> action, WaitHandle delayHandle, CancellationToken? token = null)
        {
            service.WithLock(key, wait, Functional.ToFunc<CancellationToken, Boolean>(action), delayHandle, token);
        }
    }
}