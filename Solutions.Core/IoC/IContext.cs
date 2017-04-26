using System;

namespace Solutions.Core.IoC
{
    public interface IContext : IDisposable
    {
        event Action<IContext> Disposing;
    }
}