using System;

namespace Solutions.Core.DAL
{
    public interface IDataManager : IDisposable
    {
        T GetRepository<T>() where T : class;
    }
    public interface ITransactionDataManager : IDataManager, IUnitOfWork
    {
    }
}