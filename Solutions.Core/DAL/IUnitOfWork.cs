using System;

namespace Solutions.Core.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
    }
}