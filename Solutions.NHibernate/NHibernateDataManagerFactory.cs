using System;
using System.Data;
using Solutions.Core.DAL;
using Solutions.Core.Locator;

namespace Solutions.NHibernate
{
    public abstract class NHibernateDataManagerFactory : IDataManagerFactory
    {
        private readonly Lazy<ILocator> locator;
        protected NHibernateDataManagerFactory()
        {
            locator = new Lazy<ILocator>(InitLocator);
        }

        protected abstract ILocator InitLocator();

        public IDataManager GetManager()
        {
            return new NHibernateDataManager(() => locator.Value);
        }

        public ITransactionDataManager GetTransactionManager(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            return new NHibernateDataManager(() => locator.Value, level);
        }
    }
}