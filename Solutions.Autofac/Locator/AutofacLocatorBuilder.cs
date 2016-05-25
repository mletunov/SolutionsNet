using System;
using Autofac;
using Solutions.Core.Locator;

namespace Solutions.Autofac.Locator
{
    public class AutofacLocatorBuilder : ContainerBuilder, ILocatorBuilder
    {
        private readonly ILifetimeScope basedScope;

        public AutofacLocatorBuilder() { }
        public AutofacLocatorBuilder(ILifetimeScope basedScope)
        {
            this.basedScope = basedScope;
        }

        public void Register(Type type, Type concrete)
        {
            this.RegisterType(concrete).As(type);
        }

        public void RegisterSingle(Type type, Type concrete)
        {
            this.RegisterType(concrete).As(type).SingleInstance();
        }

        public void RegisterSingle(Type type, Type concrete, Object instance)
        {
            this.RegisterInstance(instance).As(type).SingleInstance();
        }

        public ILocator Build()
        {
            ILifetimeScope container;

            if (basedScope != null)
            {
                Update(basedScope.ComponentRegistry);
                container = basedScope;
            }
            else
                container = base.Build();

            return new AutofacLocator(container);
        }
    }
}