﻿using System;
using System.Linq;
using System.Threading;
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
            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void NonBlockingStop()
        {
            var worker = GetWorker(token => true, TimeSpan.FromMinutes(1));
            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Core.Functional.Wait(worker.Stop, TimeSpan.FromSeconds(5));
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

            Assert.AreEqual(WorkerStatus.Idle, worker.Status);

            Core.Functional.Wait(worker.Start, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.Pending, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Running, worker.Status);

            Core.Functional.Wait(worker.Stop, TimeSpan.FromSeconds(5));
            Assert.AreEqual(WorkerStatus.Cancelling, worker.Status);

            manual.Set();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void PrepareExceptionStops()
        {
            var exception = new Exception();
            var worker = GetWorker(token => true, TimeSpan.FromMinutes(1), token =>
            {
                throw exception;
            });

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Start().Wait(TimeSpan.FromSeconds(5)));

            Assert.AreSame(exception, thrown.InnerExceptions.First());
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        [Test]
        public void IterationExceptionStops()
        {
            var exception = new Exception();
            var worker = GetWorker(token =>
            {
                throw exception;
            }, TimeSpan.FromMinutes(1));

            var thrown = Assert.Throws<AggregateException>(() =>
                worker.Start().Wait(TimeSpan.FromSeconds(5)));

            Assert.AreSame(exception, thrown.InnerExceptions.First());
            Assert.AreEqual(WorkerStatus.Idle, worker.Status);
        }

        private static IWorker GetWorker(Func<CancellationToken, Boolean> func, TimeSpan delay,
            Action<CancellationToken> prepare = null)
        {
            var counter = 0;
            var trigger = new Trigger(() => counter++ == 0 ? TimeSpan.Zero : delay);
            return new Core.Worker.Worker(trigger, func, prepare ?? (token => { }));
        }
    }
}