using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Solutions.Core.DAL;
using Solutions.Core.Specifications;
using Solutions.Core.Time;

namespace Solutions.Core.Queue.Db
{
    public class DbQueueService : IQueueService
    {
        private readonly Lazy<IQueueRepository> repository;
        private readonly Lazy<ITimeService> timeService;
        private readonly Lazy<FreeQueueItem> free;

        class FreeQueueItem : Specification<QueueItem>
        {
            private readonly Lazy<ITimeService> timeService;
            public FreeQueueItem(Func<ITimeService> timeService)
            {
                this.timeService = new Lazy<ITimeService>(timeService);
            }

            public override Expression<Func<QueueItem, Boolean>> Predicate
            {
                get { return item => item.HoldOn <= timeService.Value.Now; }
            }            
        }

        public DbQueueService(Func<IQueueRepository> repository,
            Func<ITimeService> timeService)
        {
            this.repository = new Lazy<IQueueRepository>(repository);
            this.timeService = new Lazy<ITimeService>(timeService);
            free = new Lazy<FreeQueueItem>(() => new FreeQueueItem(timeService));
        }

        public void AddMessage(String text)
        {
            var item = new QueueItem
            {
                Text = text,
                HoldOn = timeService.Value.Now
            };

            repository.Value.Save(item);
        }

        public QueueMessage GetMessage(TimeSpan timeout)
        {
            Func<QueueMessage> accureMessage = () =>
            {
                var item = repository.Value.GetList().Where(free.Value).
                    OrderBy(i => i.HoldOn).ThenBy(i => i.Id).FirstOrDefault();

                if (item == null)
                    return null;

                Debug.WriteLine("Accuire " + item.Text);

                item.HoldOn = timeService.Value.Now + timeout;
                return repository.Value.Save(item);
            };

            return Functional.Reliable(accureMessage, ex => ex is ConcurrencyException,
                (i, ex) => TimeSpan.Zero, new CancellationToken());
        }

        public QueueMessage PostponeMessage(QueueMessage message, TimeSpan timeout)
        {
            var item = repository.Value.Get(((QueueItem) message).Id);
            item.HoldOn = timeService.Value.Now + timeout;

            return repository.Value.Save(item);
        }

        public void DeleteMessage(QueueMessage message)
        {
            var item = (QueueItem) message;
            repository.Value.Delete(item);
        }
    }
}