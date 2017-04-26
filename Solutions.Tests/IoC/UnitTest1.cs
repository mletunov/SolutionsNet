using System;
using NUnit.Framework;
using Solutions.Autofac.Ioc;
using Solutions.Core.IoC;

namespace Solutions.Tests.IoC
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void MultiServices()
        {
            IBuild builder = new AutofacBuilder();

            builder.Add(new Registration
            {
                Service = typeof(IA),
                Target = typeof(CommonClass)
            });
            builder.Add(new Registration
            {
                Service = typeof(IA),
                Target = typeof(CommonClass)
            });

            builder.Add(new Registration
            {
                Service = typeof(IB),
                Target = typeof(CommonClass)
            });

            builder.Add(new Registration
            {
                Service = typeof(CommonClass),
                Target = typeof(CommonClass)
            });

            var commonScope = builder.Build();
            var a = (IA)commonScope.Resolve(typeof(IA));
            var b = (IB)commonScope.Resolve(typeof(IB));
            var c = (CommonClass)commonScope.Resolve(typeof(CommonClass));

            Assert.AreEqual(1, c.Number);
            Assert.AreSame(c, a);
            Assert.AreSame(c, b);
        }

        [Test]
        public void Test()
        {
            
        }
    }

    public interface IA
    {
        
    }

    public interface IB
    {

    }

    public class CommonClass : IA, IB
    {
        private static int counter;
        public int Number { get; }
        public CommonClass()
        {
            Number = ++counter;
        }
    }

    public class ServiceClass
    {
        private readonly IA common;
        public Func<ContextClass> Context { get; set; }

        public ServiceClass(IA common)
        {
            this.common = common;
        }
    }

    public class ModuleClass
    {
        private readonly IB common;
        public Func<ContextClass> Context { get; set; }
        public Func<ServiceClass> Service { get; set; }

        public ModuleClass(IB common)
        {            
            this.common = common;
        }
    }

    public class RequestClass
    {
        private readonly ModuleClass module;
        private readonly ServiceClass service;
        private readonly IA common;
        private readonly ContextClass context;

        public RequestClass(ModuleClass module, ServiceClass service, IA common, ContextClass context)
        {
            this.module = module;
            this.service = service;
            this.common = common;
            this.context = context;
        }
    }

    public class ContextClass
    {

    }
}
