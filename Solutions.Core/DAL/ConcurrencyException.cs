using System;

namespace Solutions.Core.DAL
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException(Exception ex)
            : base("Concurrency exception", ex) { }
    }
}