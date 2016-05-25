using System;
using Solutions.Core.Queue.Db;

namespace Solutions.Tests.Queue.NHibernate
{
    class QueueItemDto
    {
        public virtual Int32 Id { get; set; }
        public virtual String Text { get; set; }
        public virtual DateTime HoldOn { get; set; }
        public virtual Byte[] Version { get; set; }

        public static explicit operator QueueItemDto(QueueItem item)
        {
            return new QueueItemDto
            {
                Id = item.Id,
                Text = item.Text,
                HoldOn = item.HoldOn,
                Version = item.Version != null ? Convert.FromBase64String(item.Version) : null
            };
        }
        public static explicit operator QueueItem(QueueItemDto item)
        {
            return new QueueItem
            {
                Id = item.Id,
                Text = item.Text,
                HoldOn = item.HoldOn,
                Version = Convert.ToBase64String(item.Version)
            };
        }
    }
}
