using System;
using System.Linq;
using System.Linq.Expressions;
using Solutions.Core.DAL;
using Solutions.Core.Specifications;
using Solutions.Core.Time;

namespace Solutions.Core.Queue.Db
{
    public class DbQueueService : IQueueService
    {
        private readonly Lazy<IQueueRepository> repository;
        private readonly Lazy<ITimeService> timeService;
        private readonly Lazy<BusyQueueItem> busy;

        class BusyQueueItem : Specification<QueueItem>
        {
            private readonly Lazy<ITimeService> timeService;
            public BusyQueueItem(Func<ITimeService> timeService)
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
            busy = new Lazy<BusyQueueItem>(() => new BusyQueueItem(timeService));
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
                var item = repository.Value.GetList().Where(busy.Value).
                    OrderBy(i => i.HoldOn).FirstOrDefault();

                if (item == null)
                    return null;

                item.HoldOn = timeService.Value.Now + timeout;
                return repository.Value.Save(item);
            };

            var count = 0;
            do
            {
                try
                {
                    return accureMessage();
                }
                catch (ConcurrencyException)
                {
                    ++count;
                }
            } while (count < 3);

            return null;
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