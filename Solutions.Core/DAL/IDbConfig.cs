using System;

namespace Solutions.Core.DAL
{
    public interface IDbConfig
    {
        String ConnectionString { get; }
    }
}