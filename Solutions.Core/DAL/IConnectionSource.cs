using System;

namespace Solutions.Core.DAL
{
    public interface IConnectionSource<out TCon>
    {
        T WithConnection<T>(Func<TCon, T> func);
    }

    public static class ConnectionSourceExtension
    {
        public static void WithConnection<TCon>(this IConnectionSource<TCon> source, Action<TCon> action)
        {
            source.WithConnection(con =>
            {
                action(con);
                return true;
            });
        }
    }
}