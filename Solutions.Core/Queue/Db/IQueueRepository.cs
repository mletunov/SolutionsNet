using System;
using System.Collections.Generic;

namespace Solutions.Core.Queue.Db
{
    public interface IQueueRepository
    {
        ICollection<QueueItem> GetList();
        QueueItem Get(Int32 id);
        QueueItem Save(QueueItem item);
        void Delete(QueueItem item);
    }
}