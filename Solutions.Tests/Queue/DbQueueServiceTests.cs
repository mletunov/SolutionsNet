using Autofac;
using NHibernate;
using NUnit.Framework;
using Solutions.Core;
using Solutions.Core.DAL;
using Solutions.Core.Queue;
using Solutions.Core.Queue.Db;
using Solutions.NHibernate;
using Solutions.Tests.Queue.NHibernate;

namespace Solutions.Tests.Queue
{
    [TestFixture]
    public class DbQueueServiceTests : QueueServiceTests
    {
        public DbQueueServiceTests()
            : base(InitContainer())
        {
        }

        private static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(StubFactory.GetCurrentProvider()).SingleInstance();
            builder.RegisterInstance(StubFactory.GetDbConfig()).SingleInstance();
            builder.Register(c =>
            {
                var dbConfig = c.Resolve<IDbConfig>();
                return new NHibernateSessionSource(() => DataManagerFactory.CreateSessionFactory(dbConfig));
            }).As<IConnectionSource<ISession>>().SingleInstance();

            builder.RegisterType<QueueRepository>().As<IQueueRepository>().As<IClearable>().SingleInstance();
            builder.RegisterType<DbQueueService>().As<IQueueService>().SingleInstance();

            return builder.Build();
        }

        [SetUp]
        public void CleanUp()
        {
            var repository = container.Resolve<IClearable>();
            repository.Clear();
        }
    }
}