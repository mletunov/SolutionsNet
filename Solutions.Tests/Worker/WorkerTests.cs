using System;
using System.Linq;
using System.Threading;
#if !V4_0
using System.Threading.Tasks;
#endif
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
            var worker = GetWorker(token => { }, TimeSpan.FromMinutes(1));
            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void NonBlockingStop()
        {
            var worker = GetWorker(token => { }, TimeSpan.FromMinutes(1));
            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Core.Functional.Wait(worker.Stop, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void NonBlockingCancel()
        {
            var worker = GetWorker(token => { }, TimeSpan.FromMinutes(1));
            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Core.Functional.Wait(worker.Cancel, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void CheckStatusesIfStop()
        {
            var manual = new ManualResetEventSlim(false);

            var worker = GetWorker(token => manual.Wait(TimeSpan.FromSeconds(10), new CancellationToken()),
                TimeSpan.FromMinutes(1), token =>
                {
                    manual.Wait(token);
                    manual.Reset();
                });

            Assert.AreEqual(WorkerStatus.Idle, worker.Status);

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.StartPending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Running, worker.Status);

            Core.Functional.Wait(worker.Stop, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.StopPending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void CheckStatusesIfCancel()
        {
            var manual = new ManualResetEventSlim(false);

            var worker = GetWorker(token => manual.Wait(TimeSpan.FromSeconds(10), new CancellationToken()),
                TimeSpan.FromMinutes(1), token =>
                {
                    manual.Wait(token);
                    manual.Reset();
                });

            Assert.AreEqual(WorkerStatus.Idle, worker.Status);

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.StartPending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Running, worker.Status);

            Core.Functional.Wait(worker.Cancel, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.CancelPending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void PrepareExceptionStart()
        {
            var exception = new Exception();
            var worker = GetWorker(token => { }, TimeSpan.FromMinutes(1), token =>
            {
                throw exception;
            });

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Start().Wait(TimeSpan.FromSeconds(5)));

            Assert.AreSame(exception, thrown.InnerExceptions.Single());
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void IterationExceptionStop()
        {
            var exception = new Exception();
            var worker = GetWorker(token =>
            {
                throw exception;
            }, TimeSpan.FromMinutes(1));

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Stop().Wait(TimeSpan.FromSeconds(5)));

            Assert.AreSame(exception, thrown.InnerExceptions.Single());
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void NoCancellationRequestIfStop()
        {
            var worker = GetWorker(token =>
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                token.ThrowIfCancellationRequested();
            }, TimeSpan.FromMinutes(1));

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.AreEqual(WorkerStatus.Running, worker.Status);

            Core.Functional.Wait(worker.Stop, TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void CancellationRequestIfCancel()
        {
            var worker = GetWorker(token =>
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
                token.ThrowIfCancellationRequested();
            }, TimeSpan.FromMinutes(1));

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.AreEqual(WorkerStatus.Running, worker.Status);

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Cancel().Wait(TimeSpan.FromSeconds(5)));
#if !V4_0
            Assert.Throws<TaskCanceledException>(() => { throw thrown.InnerExceptions.Single(); });
#else
            Assert.Throws<OperationCanceledException>(() => { throw thrown.InnerExceptions.Single(); });
#endif
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void IterationExceptionCancel()
        {
            var exception = new Exception();
            var worker = GetWorker(token =>
            {
                throw exception;
            }, TimeSpan.FromMinutes(1));

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Cancel().Wait(TimeSpan.FromSeconds(5)));

            Assert.AreSame(exception, thrown.InnerExceptions.Single());
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        private static IWorker GetWorker(Action<CancellationToken> action, TimeSpan delay,
            Action<CancellationToken> prepare = null)
        {
            var trigger = new ZeroTrigger(delay);
            return new Core.Worker.Worker(trigger, action, prepare ?? (token => { }));
        }
    }
}
