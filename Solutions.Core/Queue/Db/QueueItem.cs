using System;

namespace Solutions.Core.Queue.Db
{
    public class QueueItem : QueueMessage
    {
        public Int32 Id { get; set; }
        public DateTime HoldOn { get; set; }
        public String Version { get; set; }

        public QueueItem(String text)
            : base(text)
        {
        }
    }
}