using System;
using System.Threading;

namespace Solutions.Core.Reliable
{
    /// <summary> Reliable function executor </summary>
    public interface IReliable
    {
        T Execute<T>(Func<T> func, CancellationToken? token = null);
    }

    public static class ReliableExtensions
    {
        public static void Execute(this IReliable reliable, Action action, CancellationToken? token = null)
        {
            reliable.Execute(Functional.ToFunc<Boolean>(action), token);
        }
        public static T Execute<T>(this IReliable reliable, Func<T> func, TimeSpan wait, CancellationToken? token = null)
        {
            return Functional.Wait(cancel => reliable.Execute(func, cancel), wait, token ?? new CancellationToken());
        }
        public static void Execute(this IReliable reliable, Action action, TimeSpan wait, CancellationToken? token = null)
        {
            reliable.Execute(Functional.ToFunc<Boolean>(action), wait, token);
        }
    }
}