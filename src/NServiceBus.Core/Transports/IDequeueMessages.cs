﻿namespace NServiceBus.Transports
{
    using System;

    //public class Dequeue

    /// <summary>
    /// Interface to implement when developing custom dequeuing strategies.
    /// </summary>
    public interface IDequeueMessages : IObservable<MessageAvailable>
    {
        /// <summary>
        /// Initializes the <see cref="IDequeueMessages"/>.
        /// </summary>
        DequeueInfo Init(DequeueSettings settings);
        
        /// <summary>
        /// Starts the dequeuing of message/>.
        /// </summary>
        void Start();
        
        /// <summary>
        /// Stops the dequeuing of messages.
        /// </summary>
        void Stop();
    }
}