using System;

namespace Solutions.Core.IoC
{
    public interface IScope : IDisposable
    {
        object Resolve(Type service);
    }
}
