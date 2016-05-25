using System;

namespace Solutions.Core.DAL
{
    public interface IConcurrencyItem
    {
        String Version { get; }
    }
}