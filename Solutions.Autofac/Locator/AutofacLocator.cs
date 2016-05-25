using System;
using Autofac;
using Solutions.Core.Locator;

namespace Solutions.Autofac.Locator
{
    public class AutofacLocator : ILocator
    {
        private readonly ILifetimeScope scope;
        public AutofacLocator(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public Boolean IsRegistered(Type type)
        {
            return scope.IsRegistered(type);
        }

        public Object Resolve(Type type)
        {
            return scope.Resolve(type);
        }

        public ILocatorBuilder Builder
        {
            get { return new AutofacLocatorBuilder(scope.BeginLifetimeScope()); }
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}