using System;
using System.Data;
using NHibernate;
using Solutions.Core.DAL;
using Solutions.Core.Locator;

namespace Solutions.NHibernate
{
    public class NHibernateDataManager : ITransactionDataManager, IConnectionSource<ISession>
    {
        private readonly Lazy<ILocator> locator;
        private readonly Lazy<ISession> session;
        private ITransaction transaction;

        public NHibernateDataManager(Func<ILocator> locator, IsolationLevel? level = null)
        {
            this.locator = new Lazy<ILocator>(() => InitLocator(locator()));
            session = new Lazy<ISession>(() => InitSession(level));
        }

        private ILocator InitLocator(ILocator container)
        {
            return container.Build(builder => builder.RegisterSingle<IConnectionSource<ISession>>(this));
        }

        private ISession InitSession(IsolationLevel? level)
        {
            var newSession = locator.Value.Resolve<ISession>();
            if (level.HasValue)
                transaction = newSession.BeginTransaction(level.Value);

            return newSession;
        }     

        public T GetRepository<T>() where T : class
        {
            return locator.Value.Resolve<T>();
        }
        public T WithConnection<T>(Func<ISession, T> func)
        {
            return func(session.Value);
        }        

        public void Commit()
        {
            if (transaction != null && transaction.IsActive)
                transaction.Commit();
        }
        public void Rollback()
        {
            if (transaction != null && transaction.IsActive)
                transaction.Rollback();
        }
        public void Dispose()
        {
            Rollback();

            if (transaction != null)
                transaction.Dispose();

            if (session.IsValueCreated)
                session.Value.Dispose();

            if (locator.IsValueCreated)
                locator.Value.Dispose();
        }
    }
}