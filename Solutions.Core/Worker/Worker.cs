using System;
using System.Threading;
using System.Threading.Tasks;

namespace Solutions.Core.Worker
{
    public class Worker
    {
        class Status
        {
            public CancellationTokenSource Source { get; set; }
            public Task Action { get; set; }

            public Boolean IsRunning()
            {
                return Action != null && !Action.IsCompleted &&
                       Source != null && !Source.IsCancellationRequested;
            }
        }

        private readonly Lazy<IScheduler> scheduler;
        private readonly Status status;

        public Worker(Func<IScheduler> scheduler)
        {
            this.scheduler = new Lazy<IScheduler>(scheduler);
            status = new Status();
        }

        public void Start()
        {
            lock (status)
            {
                if (!status.IsRunning())
                {

                    status.Source = new CancellationTokenSource();
#if !V4_0
                    status.Action = Task.Run(() =>
#else
                    status.Action = Task.Factory.StartNew(() =>
#endif
                    {
                        var next = scheduler.Value.Next();
                        while (!status.Source.Token.WaitHandle.WaitOne(next.Item1))
                        {
                            next.Item2(status.Source.Token);
                            next = scheduler.Value.Next();
                        }
                    });
                }
            }

            status.Action.Wait();
        }
        public void Stop()
        {
            lock (status)
            {
                if (status.Source != null)
                    status.Source.Cancel();                
            }

            if (status.Action != null)
                status.Action.Wait();
        }
    }
}
