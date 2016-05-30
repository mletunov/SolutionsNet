using System;

namespace Solutions.Core.Reliable
{
    public interface IRetryStrategy
    {
        /// <summary> Strategy to determine whether exception is transient or not </summary>
        Boolean IsTransient(Exception ex);
    }
}