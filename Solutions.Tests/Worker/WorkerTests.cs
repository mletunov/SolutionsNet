using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Solutions.Core.Worker;

namespace Solutions.Tests.Worker
{
    [TestFixture]
    public class WorkerTests
    {
        [Test]
        public void NonBlockingStart()
        {
            var worker = GetWorker(token => true, TimeSpan.FromMinutes(1));
            Wait(() => worker.Start(), TimeSpan.FromSeconds(5));
        }

        [Test]
        public void NonBlockingStop()
        {
            var worker = GetWorker(token => true, TimeSpan.FromMinutes(1));
            Wait(() => worker.Start(), TimeSpan.FromSeconds(5));
            Wait(() => worker.Stop(), TimeSpan.FromSeconds(5));
        }

        [Test]
        public void CheckStatuses()
        {
            var manual = new ManualResetEventSlim(false);

            var worker = GetWorker(token => manual.Wait(TimeSpan.FromSeconds(10), new CancellationToken()),
                TimeSpan.FromMinutes(1), token =>
                {
                    manual.Wait(token);
                    manual.Reset();
                });

            Assert.AreEqual(Core.Worker.Worker.StatusType.Ready, worker.Status);

            Wait(() => worker.Start(), TimeSpan.FromSeconds(5));
            Assert.AreEqual(Core.Worker.Worker.StatusType.Pending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(Core.Worker.Worker.StatusType.Running, worker.Status);

            Wait(() => worker.Stop(), TimeSpan.FromSeconds(5));
            Assert.AreEqual(Core.Worker.Worker.StatusType.Cancelling, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(Core.Worker.Worker.StatusType.Ready, worker.Status);
        }

        /*
        [Test]
        public void PrepareExceptionStops()
        {
            var exception = new Exception();
            var worker = GetWorker(token => true, TimeSpan.FromMinutes(1), token =>
            {
                throw exception;
            });

            var task = new Task(() => { });
            Wait(() => task = worker.Start(), TimeSpan.FromSeconds(5));
           // Wait(() => task.GetAwaiter().GetResult(), TimeSpan.FromSeconds(5));

            
            var ex = Assert.Throws<Exception>(() => Wait(() => task.GetAwaiter().GetResult(), TimeSpan.FromSeconds(5)));
            Assert.AreSame(exception, ex);
            Assert.AreEqual(Core.Worker.Worker.StatusType.Ready, worker.Status);
        }*/

        private Core.Worker.Worker GetWorker(Func<CancellationToken, Boolean> func, TimeSpan delay, Action<CancellationToken> prepare = null)
        {
            var scheduler = new FixedDelayScheduler(func, delay);
            return new Core.Worker.Worker(() => scheduler, prepare ?? (token => { }));
        }

        private void Wait(Action action, TimeSpan timeout)
        {
            Func<CancellationToken, Boolean> func = token =>
            {
                var thread = new Thread(() => action());
                thread.Start();
                var finished = thread.Join(timeout);

                if (!finished)
                    thread.Abort();

                token.ThrowIfCancellationRequested();
                return finished;
            };

            Core.Functional.Wait(func, timeout, new CancellationToken());
        }
    }
}
