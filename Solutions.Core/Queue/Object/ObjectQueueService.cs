using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Solutions.Core.Queue.Object
{
    public class ObjectQueueService : IQueueService, IClearable
    {
        class InnerMessage
        {
            public DateTime HoldOn { get; set; }
            public QueueMessage Message { get; private set; }

            private readonly String text;
            public InnerMessage(String text)
            {
                this.text = text;
            }
            public QueueMessage UpdateMessage()
            {
                Message = new QueueMessage {Text = text};
                return Message;
            }
        }

        private readonly ICollection<InnerMessage> messages = new Collection<InnerMessage>();
  
        public void AddMessage(String text)
        {
            var inner = new InnerMessage(text) {HoldOn = DateTime.UtcNow};
            lock (messages)
            {
                messages.Add(inner);
            }            
        }

        public QueueMessage GetMessage(TimeSpan timeout)
        {
            lock (messages)
            {
                var inner = messages.FirstOrDefault(m => m.HoldOn <= DateTime.UtcNow);
                if (inner == null)
                    return null;

                inner.HoldOn = DateTime.UtcNow + timeout;
                return inner.UpdateMessage();
            }
        }

        public QueueMessage PostponeMessage(QueueMessage message, TimeSpan timeout)
        {
            lock (messages)
            {
                var inner = messages.First(m => ReferenceEquals(m.Message, message));

                inner.HoldOn = DateTime.UtcNow + timeout;
                return inner.UpdateMessage();
            }
        }

        public void DeleteMessage(QueueMessage message)
        {
            lock (messages)
            {
                var inner = messages.First(m => ReferenceEquals(m.Message, message));
                messages.Remove(inner);
            }
        }

        public void Clear()
        {
            lock (messages)
                messages.Clear();
        }
    }
}