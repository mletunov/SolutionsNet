using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Persister.Entity;
using Solutions.Core;
using Solutions.Core.DAL;
using Solutions.Core.Queue.Db;

namespace Solutions.Tests.Queue.NHibernate
{
    public class QueueRepository : IQueueRepository, IClearable
    {
        private readonly Lazy<IConnectionSource<ISession>> sessionSource;

        public QueueRepository(Func<IConnectionSource<ISession>> sessionSource)
        {
            this.sessionSource = new Lazy<IConnectionSource<ISession>>(sessionSource);
        }

        public ICollection<QueueItem> GetList()
        {
            return sessionSource.Value.WithConnection(session => session.Query<QueueItemDto>().ToList())
                .Select(item => (QueueItem) item).ToList();
        }

        public QueueItem Get(Int32 id)
        {
            return (QueueItem) sessionSource.Value.WithConnection(session => session.Get<QueueItemDto>(id));
        }

        public QueueItem Save(QueueItem item)
        {
            var dto = (QueueItemDto) item;
            sessionSource.Value.WithConnection(session => session.SaveOrUpdate(dto));
            return (QueueItem)dto;
        }

        public void Delete(QueueItem item)
        {
            sessionSource.Value.WithConnection(session => session.Delete((QueueItemDto) item));
        }

        public void Clear()
        {
            sessionSource.Value.WithConnection(session =>
            {
                var table = ((AbstractEntityPersister) session.SessionFactory
                    .GetClassMetadata(typeof (QueueItemDto))).TableName;
                session.CreateSQLQuery(String.Format("DELETE FROM \"{0}\"", table)).ExecuteUpdate();
            });  
        }
    }     
}
