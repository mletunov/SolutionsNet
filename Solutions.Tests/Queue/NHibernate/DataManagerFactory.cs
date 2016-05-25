using System;
using System.Reflection;
using Autofac;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Solutions.Autofac;
using Solutions.Core.DAL;
using Solutions.Core.Locator;
using Solutions.NHibernate;

namespace Solutions.Tests.Queue.NHibernate
{
    public class DataManagerFactory : NHibernateDataManagerFactory
    {
        private readonly Func<IDbConfig> config;
        public DataManagerFactory(Func<IDbConfig> config)
        {
            this.config = config;
        }

        public static ISessionFactory CreateSessionFactory(IDbConfig dbConfig)
        {
            var configuration = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(dbConfig.ConnectionString))
                .Mappings(m => m.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()))
                .BuildConfiguration();

            return configuration.BuildSessionFactory();
        }

        protected override ILocator InitLocator()
        {
            var builder = new ContainerBuilder();
            
            builder.Register(c => CreateSessionFactory(config()).OpenSession()).InstancePerLifetimeScope();
            builder.RegisterType<QueueRepository>().AsImplementedInterfaces().InstancePerLifetimeScope();

            return new AutofacLocator(builder.Build());
        }
    }
}
