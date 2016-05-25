using System;
using System.Data;

namespace Solutions.Core.DAL
{
    public interface IDataManagerFactory
    {
        IDataManager GetManager();
        ITransactionDataManager GetTransactionManager(IsolationLevel level = IsolationLevel.ReadCommitted);
    }

    public static class DataManagerExtensions
    {
        public static T WithDataManager<T>(this IDataManagerFactory factory, Func<IDataManager, T> func, IsolationLevel? level = null)
        {
            using (var manager = level == null ? factory.GetManager() : factory.GetTransactionManager(level.Value))
            {
                var result = func(manager);

                var transaction = manager as IUnitOfWork;
                if (transaction != null)
                    transaction.Commit();

                return result;
            }
        }
        public static void WithDataManager(this IDataManagerFactory factory, Action<IDataManager> func, IsolationLevel? level = null)
        {
            factory.WithDataManager(manager =>
            {
                func(manager);
                return true;
            }, level);
        }

        public static T WithTransaction<T>(this IDataManagerFactory factory, Func<ITransactionDataManager, T> func, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            using (var manager = factory.GetTransactionManager(level))
            {
                var result = func(manager);

                manager.Commit();
                return result;
            }
        }
        public static void WithTransaction(this IDataManagerFactory factory, Action<ITransactionDataManager> func, IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            factory.WithTransaction(manager =>
            {
                func(manager);
                return true;
            }, level);
        }

        public static T WithRepository<TRepo, T>(this IDataManagerFactory factory, Func<TRepo, T> func, IsolationLevel? level = null) where TRepo : class
        {
            return factory.WithDataManager(manager => manager.WithRepository(func), level);
        }
        public static void WithRepository<TRepo>(this IDataManagerFactory factory, Action<TRepo> func, IsolationLevel? level = null) where TRepo : class
        {
            factory.WithDataManager(manager => manager.WithRepository(func), level);
        }

        public static T WithRepository<TRepo, T>(this IDataManager manager, Func<TRepo, T> func) where TRepo : class
        {
            var repository = manager.GetRepository<TRepo>();
            return func(repository);
        }
        public static void WithRepository<TRepo>(this IDataManager manager, Action<TRepo> func) where TRepo : class
        {
            manager.WithRepository<TRepo, Boolean>(repo =>
            {
                func(repo);
                return true;
            });
        }
    }
}