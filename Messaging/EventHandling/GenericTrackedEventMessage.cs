using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public class GenericTrackedEventMessage<T> : GenericTrackedEventMessage, ITrackedEventMessage<T>
    {
        public GenericTrackedEventMessage(ITrackingToken trackingToken, IEventMessage @delegate) : base(trackingToken, @delegate)
        {
        }

        public GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage @delegate, Func<DateTime> timestamp) : base(trackingToken, @delegate, timestamp)
        {
        }

        internal GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage @delegate, DateTime timestamp) : base(trackingToken, @delegate, timestamp)
        {
        }

        public new T Payload => (T)base.Payload;
    }

    public class GenericTrackedEventMessage : GenericEventMessage, ITrackedEventMessage
    {
        public GenericTrackedEventMessage(ITrackingToken trackingToken, IEventMessage @delegate) : base(@delegate, @delegate.Timestamp)
        {
            TrackingToken = trackingToken;
        }
        public GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage @delegate, Func<DateTime> timestamp) : base(@delegate, timestamp)
        {
            TrackingToken = trackingToken;
        }
        internal GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage @delegate, DateTime timestamp) : base(@delegate, timestamp)
        {
            TrackingToken = trackingToken;
        }

        protected override void DescribeTo(StringBuilder stringBuilder)
        {
            base.DescribeTo(stringBuilder);
            stringBuilder.Append(", trackingToken={")
                .Append(TrackingToken)
                .Append('}');
        }

        protected override string DescribeType => "GenericTrackedEventMessage";

        public ITrackingToken TrackingToken { get; internal set; }
    }
}