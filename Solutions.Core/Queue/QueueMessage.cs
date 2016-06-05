using System;

namespace Solutions.Core.Queue
{
    public class QueueMessage
    {
        public String Text { get; private set; }

        public QueueMessage(String text)
        {
            Text = text;
        }
    }
}