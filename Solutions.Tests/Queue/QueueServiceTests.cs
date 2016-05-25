using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NUnit.Framework;
using Solutions.Core.Queue;

namespace Solutions.Tests.Queue
{
    public abstract class QueueServiceTests
    {
        protected readonly IContainer container;
        protected QueueServiceTests(IContainer container)
        {
            this.container = container;
        }

        [Test]
        public void NullReturnIfQueueIsEmpty()
        {
            var service = container.Resolve<IQueueService>();

            var message = service.GetMessage(TimeSpan.Zero);
            Assert.IsNull(message);
        }

        [Test]
        public void MessageReturnIfQueueIsNotEmpty()
        {
            var service = container.Resolve<IQueueService>();

            const String text = "TestMessage";
            service.AddMessage(text);

            var message = service.GetMessage(TimeSpan.FromMinutes(2));
            Assert.AreEqual(text, message.Text);
        }

        [Test]
        public void SetMessageBusy()
        {
            var service = container.Resolve<IQueueService>();

            const String text = "TestMessage";
            var timeout = TimeSpan.FromSeconds(2);

            service.AddMessage(text);
            var message1 = service.GetMessage(timeout);
            var message2 = service.GetMessage(timeout);

            Assert.AreEqual(text, message1.Text);
            Assert.IsNull(message2);

            Thread.Sleep(timeout.Add(TimeSpan.FromSeconds(1)));

            message2 = service.GetMessage(timeout);
            Assert.AreEqual(text, message2.Text);
        }

        [Test]
        public void ExclusiveTest()
        {
            var service = container.Resolve<IQueueService>();

            const String text = "TestMessage";
            const Int32 maxCount = 5;
            var timeout = TimeSpan.FromSeconds(2);

            service.AddMessage(text);
            var message = service.GetMessage(timeout);
            Task.WaitAll(
                Task.Run(() =>
                {
                    var count = 0;
                    do
                    {
                        Thread.Sleep(timeout.Subtract(TimeSpan.FromSeconds(1)));
                        message = service.PostponeMessage(message, timeout);
                    } while (++count < maxCount);
                }),
                Task.Run(() =>
                {
                    var count = 0;
                    do
                    {
                        Thread.Sleep(timeout.Subtract(TimeSpan.FromSeconds(1)));
                        Assert.IsNull(service.GetMessage(timeout));
                    } while (++count < maxCount);
                }));
        }

        [Test]
        public void ConcurrencyTest()
        {
            var service = container.Resolve<IQueueService>();
            const String text1 = "TestMessage1";
            const String text2 = "TestMessage2";

            service.AddMessage(text1);
            service.AddMessage(text2);
            var timeout = TimeSpan.FromSeconds(2);

            var tasks = Enumerable.Range(1, 10).Select(i => Task.Run(() => service.GetMessage(timeout) != null ? 1 : 0))
                .ToArray();
            Task.WaitAll(tasks.Cast<Task>().ToArray());
            Assert.AreEqual(2, tasks.Select(t => t.Result).Sum(r => r));
        }

        [Test]
        public void DeleteTest()
        {
            var service = container.Resolve<IQueueService>();
            const String text = "TestMessage";
            var timeout = TimeSpan.FromSeconds(2);

            service.AddMessage(text);
            var message = service.GetMessage(timeout);
            service.DeleteMessage(message);

            Thread.Sleep(timeout.Add(TimeSpan.FromSeconds(1)));
            Assert.IsNull(service.GetMessage(timeout));
        }

        [Test]
        public void PostponeDoesnotUpdateMessage()
        {
            var service = container.Resolve<IQueueService>();

            const String text = "TestMessage";
            const String newText = "NewMessage";
            var timeout = TimeSpan.FromSeconds(2);

            service.AddMessage(text);
            var message = service.GetMessage(timeout);

            message.Text = newText;
            var postponedMessage = service.PostponeMessage(message, timeout);

            Assert.AreEqual(text, postponedMessage.Text);
        }
    }
}
