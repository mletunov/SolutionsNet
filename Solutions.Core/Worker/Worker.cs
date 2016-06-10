using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core.Worker
{
    public class Worker
    {       
        public enum StatusType
        {
            Idle,
            Running,
            Pending,
            Cancelling,
        }

        private readonly Lazy<IScheduler> scheduler;
        private readonly Action<CancellationToken> prepare;

        private Task task;
        private CancellationTokenSource cancellation;
        public StatusType Status { get; private set; }

        public Worker(Func<IScheduler> scheduler)
            : this(scheduler, token => { })
        {
        }

        public Worker(Func<IScheduler> scheduler, Action<CancellationToken> prepare)
        {
            this.prepare = prepare;
            this.scheduler = new Lazy<IScheduler>(scheduler);
            Status = StatusType.Idle;
        }

        private Action Execute()
        {
            return () =>
            {
                try
                {
                    prepare(cancellation.Token);
                    Status = StatusType.Running;
                
                    var next = scheduler.Value.Next();
                    while (!cancellation.Token.WaitHandle.WaitOne(next.Item1))
                    {
                        next.Item2(cancellation.Token);
                        next = scheduler.Value.Next();
                    }
                }
                finally
                {
                    Status = StatusType.Idle;
                }
            };
        }

#if !V4_0
        public async Task Start()
        {
            lock (scheduler)
            {
                if (Status == StatusType.Idle)
                {
                    Status = StatusType.Pending;
                    cancellation = new CancellationTokenSource();

                    task = Task.Run(Execute());
                }
            }

            await task;           
        }

        public async Task Stop()
        {
            lock (scheduler)
            {
                if (Status == StatusType.Running || Status == StatusType.Pending)
                {
                    Status = StatusType.Cancelling;
                    cancellation.Cancel();
                }
            }

            await task;
        }
#else
        public Task Start()
        {
            lock (scheduler)
            {
                if (Status != StatusType.Idle)
                    return task;

                Status = StatusType.Pending;
                cancellation = new CancellationTokenSource();

                return task = Task.Factory.StartNew(Execute());
            }
        }

        public Task Stop()
        {
            lock (scheduler)
            {
                if (Status != StatusType.Running && Status != StatusType.Pending)
                    return task;

                Status = StatusType.Cancelling;
                cancellation.Cancel();
                return task;
            }
        }
#endif
    }
}
