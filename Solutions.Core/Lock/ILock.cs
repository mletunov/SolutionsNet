using System;
using System.Threading;

namespace Solutions.Core.Lock
{
    public interface ILock : IDisposable
    {
        CancellationToken Token { get; }
    }
}