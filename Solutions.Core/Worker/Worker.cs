using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core.Worker
{
    public class Worker : IWorker
    {       
        private readonly Func<TimeSpan> trigger;
        private readonly Action<CancellationToken> prepare;
        private readonly Func<CancellationToken, Boolean> func;

        private Task task;
        private CancellationTokenSource cancellation;
        public WorkerStatus Status { get; private set; }

        private Object critical = new Object();

        public Worker(ITrigger trigger, Func<CancellationToken, Boolean> func)
            : this(trigger, func, token => { })
        { }

        public Worker(Func<TimeSpan> trigger, Func<CancellationToken, Boolean> func)
            : this(trigger, func, token => { })
        { }

        public Worker(ITrigger trigger, Func<CancellationToken, Boolean> func, Action<CancellationToken> prepare)
            : this(trigger.Next, func, prepare)
        { }

        public Worker(Func<TimeSpan> trigger, Func<CancellationToken, Boolean> func, Action<CancellationToken> prepare)
        {
            this.prepare = prepare;
            this.func = func;
            this.trigger = trigger;

            Status = WorkerStatus.Idle;
        }

        private Action Execute()
        {
            return () =>
            {
                try
                {
                    prepare(cancellation.Token);
                    Status = WorkerStatus.Running;

                    while (!cancellation.Token.WaitHandle.WaitOne(trigger()))
                        func(cancellation.Token);
                }
                finally
                {
                    Status = WorkerStatus.Idle;
                }
            };
        }

#if !V4_0
        public async Task Start()
        {
            lock (critical)
            {
                if (Status == WorkerStatus.Idle)
                {
                    Status = WorkerStatus.Pending;
                    cancellation = new CancellationTokenSource();

                    task = Task.Run(Execute());
                }
            }

            await task;           
        }

        public async Task Stop()
        {
            lock (critical)
            {
                if (Status == WorkerStatus.Running || Status == WorkerStatus.Pending)
                {
                    Status = WorkerStatus.Cancelling;
                    cancellation.Cancel();
                }
            }

            await task;
        }
#else
        public Task Start()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Idle)
                    return task;

                Status = WorkerStatus.Pending;
                cancellation = new CancellationTokenSource();

                return task = Task.Factory.StartNew(Execute());
            }
        }

        public Task Stop()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Running && Status != WorkerStatus.Pending)
                    return task;

                Status = WorkerStatus.Cancelling;
                cancellation.Cancel();
                return task;
            }
        }
#endif
    }
}
