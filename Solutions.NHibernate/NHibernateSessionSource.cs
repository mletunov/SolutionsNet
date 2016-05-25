using System;
using NHibernate;
using Solutions.Core.DAL;

namespace Solutions.NHibernate
{
    public class NHibernateSessionSource : IConnectionSource<ISession>
    {
        private readonly Lazy<ISessionFactory> factory;
        public NHibernateSessionSource(Func<ISessionFactory> factory)
        {
            this.factory = new Lazy<ISessionFactory>(factory);
        }

        public T WithConnection<T>(Func<ISession, T> func)
        {
            try
            {
                using (var session = factory.Value.OpenSession())
                {
                    var result = func(session);

                    session.Flush();
                    return result;
                }
            }
            catch (StaleObjectStateException ex)
            {
                throw new ConcurrencyException(ex);
            }
        }
    }
}