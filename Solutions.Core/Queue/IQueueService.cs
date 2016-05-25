using System;

namespace Solutions.Core.Queue
{
    /// <summary> Reliable queue service </summary>
    public interface IQueueService
    {
        /// <summary> Add message to queue </summary>       
        void AddMessage(String text);

        /// <summary> Get message from queue for timeout </summary>
        QueueMessage GetMessage(TimeSpan timeout);

        /// <summary> Postpone message for timeout </summary>        
        QueueMessage PostponeMessage(QueueMessage message, TimeSpan timeout);

        /// <summary> Delete message from queue </summary>        
        void DeleteMessage(QueueMessage message);
    }
}