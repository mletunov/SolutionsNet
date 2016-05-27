using System;
using System.Threading;
using NUnit.Framework;
using Func = Solutions.Core.Functional;

namespace Solutions.Tests.Functional
{
    [TestFixture]
    public class ReliableTests
    {
        [Test]
        public void HappyPath()
        {
            var obj = new Object();
            Func<Object> func = () => obj;
            var result = Func.Reliable(func, null, null, new CancellationToken());

            Assert.AreSame(obj, result);
        }

        [Test]
        public void RetryOnlyOnException()
        {
            var retryIsCalled = false;
            Func<Object> successFunc = () => new Object();
            Func<Object> failFunc = () =>
            {
                if (!retryIsCalled)
                    throw new Exception();
                return successFunc();
            };
            Func<Exception, Boolean> retry = exception =>
            {
                Assert.IsFalse(retryIsCalled);
                retryIsCalled = true;
                return true;
            };

            Func.Reliable(successFunc, retry, null, new CancellationToken());
            Assert.IsFalse(retryIsCalled);

            Func.Reliable(failFunc, retry, (i, exception) => TimeSpan.Zero, new CancellationToken());
            Assert.IsTrue(retryIsCalled);
        }

        [Test]
        public void DelayOnlyOnRetry()
        {
            var delayIsCalled = false;
            var exception = new Exception();

            Func<Object> successFunc = () => new Object();
            Func<Object> failFunc = () =>
            {
                if (!delayIsCalled)
                    throw exception;
                return successFunc();
            };
            Func<Int32, Exception, TimeSpan?> delay = (i, ex) =>
            {
                Assert.IsFalse(delayIsCalled);
                delayIsCalled = true;
                return TimeSpan.Zero;
            };

            Func.Reliable(successFunc, ex => true, delay, new CancellationToken());
            Assert.IsFalse(delayIsCalled);

            try
            {
                Func.Reliable(failFunc, ex => false, delay, new CancellationToken());
                Assert.IsFalse(delayIsCalled);
            }
            catch (Exception ex)
            {
                Assert.AreSame(exception, ex);
            }

            Func.Reliable(failFunc, ex => true, delay, new CancellationToken());
            Assert.IsTrue(delayIsCalled);
        }

        [Test]
        public void ExceptionTheSame()
        {
            var count = 0;
            var exception = new Exception();

            Func<Object> func = () =>
            {
                if (count < 1)
                    throw exception;
                return new Object();
            };

            Exception retryException = null;
            Exception delayException = null;

            Func.Reliable(func, ex =>
            {
                retryException = ex;
                return true;
            }, (i, ex) =>
            {
                count = i;
                delayException = ex;
                return TimeSpan.Zero;
            }, new CancellationToken());

            Assert.AreSame(exception, retryException);
            Assert.AreSame(exception, delayException);
        }

        [Test]
        public void RetryCountsFromZero()
        {
            var count = 0;
            Func<Object> func = () =>
            {
                if (count < 3)
                    throw new Exception();
                return new Object();
            };

            Func.Reliable(func, ex => true, (i, ex) =>
            {
                Assert.AreEqual(count, i);
                count++;
                return TimeSpan.Zero;
            }, new CancellationToken());

            Assert.AreEqual(3, count);
        }

        [Test]
        public void ExceptionIfRetryIsNotNeeded()
        {
            var exception = new Exception();
            Func<Object> func = () =>
            {
                throw exception;
            };

            try
            {
                Func.Reliable(func, null, null, new CancellationToken());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreSame(exception, ex);
            }

            try
            {
                Func.Reliable(func, ex => false, null, new CancellationToken());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreSame(exception, ex);
            }
        }
    }
}
