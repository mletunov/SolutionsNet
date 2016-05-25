using Autofac;
using NUnit.Framework;
using Solutions.Core;
using Solutions.Core.Queue.Object;

namespace Solutions.Tests.Queue
{
    [TestFixture]
    public class ObjectQueueServiceTests : QueueServiceTests
    {
        public ObjectQueueServiceTests() 
            : base(InitContainer())
        {
        }

        private static IContainer InitContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ObjectQueueService>().AsImplementedInterfaces().SingleInstance();
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