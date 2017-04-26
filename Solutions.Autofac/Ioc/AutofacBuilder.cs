using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Solutions.Core.IoC;

namespace Solutions.Autofac.Ioc
{
    public class AutofacBuilder : ContainerBuilder, IBuild
    {
        private readonly ConcurrentDictionary<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>
            typeRegistrations = new ConcurrentDictionary<Type, IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>();

        public void Add(Registration registration)
        {
            if (registration.TargetFunc != null)
            {
                this.Register(context =>
                    {
                        return registration.TargetFunc(null);
                        //var scope = (context as IInstanceLookup).Return(l => l.ActivationScope, context as ILifetimeScope);
                        //return registration.ConcreteFunc(new AutofacScope(scope));
                    })
                    .As(registration.Service)
                    //.With(r => registration.External ? r.ExternallyOwned() : r)
                    .PropertiesAutowired()
                    .InstancePerLifetimeScope();
            }
            else
            {
                var builder = typeRegistrations.GetOrAdd(registration.Target,
                    type => this.RegisterType(registration.Target)
                        .As(registration.Service)
                        .PropertiesAutowired()
                        .InstancePerLifetimeScope());

                if (!builder.RegistrationData.Services.Contains(new TypedService(registration.Service)))
                    builder.RegistrationData.AddService(new TypedService(registration.Service));

            }
        }

        IScope IBuild.Build()
        {
            return new AutofacScope(this.Build());
        }
    }

    public class AutofacScope : IScope
    {
        private readonly ILifetimeScope scope;

        public AutofacScope(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public void Dispose()
        {
            scope.Dispose();
        }

        public object Resolve(Type service)
        {
            return scope.Resolve(service);
        }
    }
}
