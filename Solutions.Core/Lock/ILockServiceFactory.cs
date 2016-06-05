using System;

namespace Solutions.Core.Lock
{
    public interface ILockServiceFactory
    {
        /// <summary> Return service that can acquire no more maxCount locks at once </summary>       
        ILockService GetService(Int32 maxCount = 1);
    }
}