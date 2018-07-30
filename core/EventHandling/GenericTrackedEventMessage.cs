using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventHandling
{
    public class GenericTrackedEventMessage<T> : GenericEventMessage<T>, ITrackedEventMessage<T>
    {
        public GenericTrackedEventMessage(ITrackingToken trackingToken, IEventMessage<T> @delegate) : base(@delegate, @delegate.Timestamp)
        {
            TrackingToken = trackingToken;
        }
        public GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage<T> @delegate, Func<DateTime> timestamp) : base(@delegate, timestamp)
        {
            TrackingToken = trackingToken;
        }
        protected GenericTrackedEventMessage(ITrackingToken trackingToken, IMessage<T> @delegate, DateTime timestamp) : base(@delegate, timestamp)
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

        public ITrackingToken TrackingToken { get; }
    }
}