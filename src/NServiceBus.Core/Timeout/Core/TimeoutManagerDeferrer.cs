﻿namespace NServiceBus.Timeout
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.Logging;
    using NServiceBus.Transports;
    using NServiceBus.Unicast.Transport;

    class TimeoutManagerDeferrer : ICancelDeferredMessages
    {
        public TimeoutManagerDeferrer(ISendMessages messageSender, string timeoutManagerAddress)
        {
            this.messageSender = messageSender;
            this.timeoutManagerAddress = timeoutManagerAddress;
        }

       
        public void Defer(OutgoingMessage message, TransportDeferOptions sendMessageOptions)
        {
            message.Headers[TimeoutManagerHeaders.RouteExpiredTimeoutTo] = sendMessageOptions.Destination;

            DateTime deliverAt;

            if (sendMessageOptions.DelayDeliveryFor.HasValue)
            {
                deliverAt = DateTime.UtcNow + sendMessageOptions.DelayDeliveryFor.Value;
            }
            else
            {
                if (sendMessageOptions.DeliverAt.HasValue)
                {
                    deliverAt = sendMessageOptions.DeliverAt.Value;    
                }
                else
                {
                    throw new ArgumentException("A delivery time needs to be specified for Deferred messages");
                }
                
            }

            message.Headers[TimeoutManagerHeaders.Expire] = DateTimeExtensions.ToWireFormattedString(deliverAt);
            
            try
            {
                messageSender.Send(message, new TransportSendOptions(timeoutManagerAddress, new AtomicWithReceiveOperation(),new List<DeliveryConstraint>()));
            }
            catch (Exception ex)
            {
                Log.Error("There was a problem deferring the message. Make sure that DisableTimeoutManager was not called for your endpoint.", ex);
                throw;
            }
        }
        public void CancelDeferredMessages(string messageKey)
        {
            var controlMessage = ControlMessageFactory.Create(MessageIntentEnum.Send);

            controlMessage.Headers["$MessageKeyToClear"] = messageKey;
            controlMessage.Headers[TimeoutManagerHeaders.ClearTimeouts] = bool.TrueString;

            messageSender.Send(controlMessage, new TransportSendOptions(timeoutManagerAddress, new NoConsistencyRequired(), new List<DeliveryConstraint>()));

        }

        ISendMessages messageSender;
        string timeoutManagerAddress;
     
        static ILog Log = LogManager.GetLogger<TimeoutManagerDeferrer>();
    }
}
