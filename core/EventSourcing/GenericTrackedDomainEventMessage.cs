using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventSourcing
{
    public class GenericTrackedDomainEventMessage<T> : GenericTrackedDomainEventMessage, ITrackedEventMessage<T>
    {
        public new T Payload { get; }

        public GenericTrackedDomainEventMessage(ITrackingToken trackingToken, IDomainEventMessage @delegate) : base(trackingToken, @delegate)
        {
        }

        public GenericTrackedDomainEventMessage(ITrackingToken trackingToken, string type, string aggregateIdentifier, long sequenceNumber, IMessage @delegate, Func<DateTime> timestamp) : base(trackingToken, type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
        }

        protected GenericTrackedDomainEventMessage(ITrackingToken trackingToken, string type, string aggregateIdentifier, long sequenceNumber, IMessage @delegate, DateTime timestamp) : base(trackingToken, type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
        }
    }
    public class GenericTrackedDomainEventMessage : GenericDomainEventMessage, ITrackedEventMessage
    {
        public GenericTrackedDomainEventMessage(ITrackingToken trackingToken, IDomainEventMessage @delegate) 
            : this(trackingToken, @delegate.Type, @delegate.AggregateIdentifier, @delegate.SequenceNumber, @delegate, @delegate.Timestamp)
        {
            
        }
        
        public GenericTrackedDomainEventMessage(ITrackingToken trackingToken, string type, string aggregateIdentifier,
            long sequenceNumber, IMessage @delegate, Func<DateTime> timestamp)
            :base(type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
            
            TrackingToken = trackingToken;
        }

        protected GenericTrackedDomainEventMessage(ITrackingToken trackingToken, String type, String aggregateIdentifier,
            long sequenceNumber, IMessage @delegate, DateTime timestamp)
            :base(type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
            
            TrackingToken = trackingToken;
        }

        public override IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            return new GenericTrackedDomainEventMessage(TrackingToken, Type, AggregateIdentifier, SequenceNumber, Delegate.WithMetaData(metaData), Timestamp);
        }
        public GenericTrackedDomainEventMessage AndMetaData(IReadOnlyDictionary<String, object> metaData) 
        {
            return new GenericTrackedDomainEventMessage(TrackingToken, Type, AggregateIdentifier, SequenceNumber, Delegate.AndMetaData(metaData), Timestamp);
        }
        
        protected void describeTo(StringBuilder stringBuilder) 
        {
            base.DescribeTo(stringBuilder);
            stringBuilder.Append(", trackingToken={")
                .Append(TrackingToken)
                .Append('}');
        }

        protected override string DescribeType => "GenericTrackedDomainEventMessage";

        public ITrackingToken TrackingToken { get; }
    }

    
}