using System;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Compatibility;
using Func = Solutions.Core.Functional;

namespace Solutions.Tests.Functional
{
    [TestFixture]
    public class WaitTests
    {
        [Test]
        public void WaitNoMoreFunction()
        {
            var obj = new Object();
            Func<CancellationToken, Object> func = token =>
            {
                token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                token.ThrowIfCancellationRequested();
                return obj;
            };

            var sw = new Stopwatch();
            sw.Start();

            var result = Func.Wait(func, TimeSpan.FromMinutes(1), new CancellationToken());
            sw.Stop();

            Assert.That(() => sw.Elapsed < TimeSpan.FromSeconds(2));
            Assert.AreSame(obj, result);
        }

        [Test]
        public void WaitNoMoreTimeout()
        {
            var obj = new Object();
            Func<CancellationToken, Object> func = token =>
            {
                token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
                return obj;
            };

            var sw = new Stopwatch();
            sw.Start();

            var result = Func.Wait(func, TimeSpan.FromSeconds(1), new CancellationToken());
            sw.Stop();

            Assert.That(() => sw.Elapsed < TimeSpan.FromSeconds(2));
            Assert.AreSame(obj, result);
        }

        [Test]
        public void TimeoutIfTokenIsCanceled()
        {
            Func<CancellationToken, Object> func = token =>
            {
                while (!token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
                {}

                token.ThrowIfCancellationRequested();
                return new Object();
            };

            Assert.Throws<TimeoutException>(() => Func.Wait(func, TimeSpan.Zero, new CancellationToken()));
        }

        [Test]
        public void NoTimeoutIfTokenIsNotUsed()
        {
            var obj = new Object();
            Func<CancellationToken, Object> func = token =>
            {
                while (!token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
                { }

                return obj;
            };

            var result = Func.Wait(func, TimeSpan.Zero, new CancellationToken());
            Assert.AreSame(obj, result);
        }

        [Test]
        public void NoWaitIfException()
        {
            var exception = new Exception();
            Func<CancellationToken, Object> func = token =>
            {
                throw exception;
            };

            var sw = new Stopwatch();
            sw.Start();

            var thrown = Assert.Throws<Exception>(() => Func.Wait(func, TimeSpan.FromSeconds(10), new CancellationToken()));

            sw.Stop();
            Assert.That(() => sw.Elapsed < TimeSpan.FromSeconds(1));
            Assert.AreSame(exception, thrown);
        }
    }
}
