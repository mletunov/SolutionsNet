using System;

namespace Solutions.Core.Worker
{
    public interface ITrigger
    {
        /// <summary> Returns delay to next fire </summary>
        TimeSpan Next();
    }
}