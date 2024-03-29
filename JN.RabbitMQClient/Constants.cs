﻿namespace JN.RabbitMQClient
{

    public static class Constants
    {
        /// <summary>
        /// OK - message is considered as successfully processed
        /// RequeueMessageWithDelay - message is removed from the queue, but sent to a retry queue for latter processing (typically with a dead letter configuration)
        /// IgnoreMessage - message is removed from the queue and ignored
        /// IgnoreMessageWithRequeue - message is rejected and sent back to the queue
        /// </summary>
        public enum MessageProcessInstruction
        {
            OK,
            RequeueMessageWithDelay,
            IgnoreMessage,
            IgnoreMessageWithRequeue,
            Unknown
        };

        internal const string FirstErrorTimeStampHeaderName = "JN-firstErrorTimeStamp";


        public enum Errors
        {
            OK = 0,
            ErrorCreatingConnection = -1,
            ErrorCreatingSendingMessage = -2,
            ErrorGettingQueueDetails = -3,
            InvalidMessage = -4
        }
    }
}
