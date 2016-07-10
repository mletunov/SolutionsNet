using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core.Worker
{
    public class Worker : IWorker
    {       
        private readonly Func<TimeSpan> trigger;
        private readonly Action<CancellationToken> prepare;
        private readonly Action<CancellationToken> action;

        private Task task;
        private CancellationTokenSource cancelSource;
        private CancellationTokenSource stopSource;
        public WorkerStatus Status { get; private set; }

        private readonly Object critical = new Object();

        public Worker(ITrigger trigger, Action<CancellationToken> action)
            : this(trigger, action, token => { })
        { }

        public Worker(Func<TimeSpan> trigger, Action<CancellationToken> action)
            : this(trigger, action, token => { })
        { }

        public Worker(ITrigger trigger, Action<CancellationToken> action, Action<CancellationToken> prepare)
            : this(trigger.Next, action, prepare)
        { }

        public Worker(Func<TimeSpan> trigger, Action<CancellationToken> action, Action<CancellationToken> prepare)
        {
            this.prepare = prepare;
            this.action = action;
            this.trigger = trigger;

            Status = WorkerStatus.Idle;
        }

        private void Prepare()
        {
            try
            {
                prepare(cancelSource.Token);
            }
            finally 
            {
                Status = WorkerStatus.Idle;
            }
        }

        private void Execute()
        {
            try
            {
                Status = WorkerStatus.Running;
                while (!stopSource.Token.WaitHandle.WaitOne(trigger()))
                    action(cancelSource.Token);
            }
            finally
            {
                Status = WorkerStatus.Idle;
            }
        }

#if !V4_0
        public async Task Start()
        {
            var result = Task.Run(() => { });
            lock (critical)
            {
                if (Status == WorkerStatus.Idle)
                {
                    cancelSource = new CancellationTokenSource();
                    stopSource = CancellationTokenSource.CreateLinkedTokenSource(new[] {cancelSource.Token});
                    Status = WorkerStatus.StartPending;

                    task = new Task(Execute);
                    result = Task.Run(() =>
                    {
                        Prepare();
                        task.Start();
                    });
                }
            }

            await result;
        }

        public async Task Stop()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Idle && Status != WorkerStatus.CancelPending && Status != WorkerStatus.StopPending)
                {
                    Status = WorkerStatus.StopPending;
                    stopSource.Cancel();
                }
            }

            await task;
        }

        public async Task Cancel()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Idle && Status != WorkerStatus.CancelPending)
                {
                    Status = WorkerStatus.CancelPending;
                    cancelSource.Cancel();
                }
            }

            await task;
        }
#else
        public Task Start()
        {
            var result = Task.Factory.StartNew(() => { });
            lock (critical)
            {
                if (Status == WorkerStatus.Idle)
                {
                    cancelSource = new CancellationTokenSource();
                    stopSource = CancellationTokenSource.CreateLinkedTokenSource(new[] {cancelSource.Token});
                    Status = WorkerStatus.StartPending;

                    task = new Task(Execute);
                    result = Task.Factory.StartNew(() =>
                    {
                        Prepare();
                        task.Start();
                    });
                }
            }

            return result;
        }

        public Task Stop()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Idle && Status != WorkerStatus.CancelPending && Status != WorkerStatus.StopPending)
                {
                    Status = WorkerStatus.StopPending;
                    stopSource.Cancel();
                } 
            }

            return task;
        }

        public Task Cancel()
        {
            lock (critical)
            {
                if (Status != WorkerStatus.Idle && Status != WorkerStatus.CancelPending)
                {
                    Status = WorkerStatus.CancelPending;
                    cancelSource.Cancel();
                }
            }

            return task;
        }
#endif
    }
}
