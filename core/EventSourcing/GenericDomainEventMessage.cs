using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging;

namespace NAxonFramework.EventSourcing
{
    public class GenericDomainEventMessage<T> : GenericDomainEventMessage, IDomainEventMessage<T>
    {

        public new T Payload => (T) base.Payload;

        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, T payload) : base(type, aggregateIdentifier, sequenceNumber, payload)
        {
        }

        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, T payload, IReadOnlyDictionary<string, object> metaData) : base(type, aggregateIdentifier, sequenceNumber, payload, metaData)
        {
        }

        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, T payload, IReadOnlyDictionary<string, object> metaData, string messageIdentifier, DateTime timestamp) : base(type, aggregateIdentifier, sequenceNumber, payload, metaData, messageIdentifier, timestamp)
        {
        }

        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, IMessage<T> @delegate, Func<DateTime> timestamp) : base(type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
        }

        protected GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, IMessage<T> @delegate, DateTime timestamp) : base(type, aggregateIdentifier, sequenceNumber, @delegate, timestamp)
        {
        }
    }
    
    public class GenericDomainEventMessage : GenericEventMessage, IDomainEventMessage
    {

        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, object payload) 
            : this(type, aggregateIdentifier, sequenceNumber, payload, Messaging.MetaData.EmptyInstance) 
        {
            
        }
        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, object payload, IReadOnlyDictionary<string, object> metaData)
            :this(type, aggregateIdentifier, sequenceNumber, new GenericMessage(payload, metaData), Clock)
        {
            
        }
        public GenericDomainEventMessage(string type, string aggregateIdentifier, long sequenceNumber, object payload, 
            IReadOnlyDictionary<string, object> metaData, string messageIdentifier, DateTime timestamp)
            : this(type, aggregateIdentifier, sequenceNumber, new GenericMessage(messageIdentifier, payload, metaData),
                timestamp)
        {
            
        }
        public GenericDomainEventMessage(String type, String aggregateIdentifier, long sequenceNumber, IMessage @delegate, Func<DateTime> timestamp) : base(@delegate, timestamp)
        {
            Type = type;
            AggregateIdentifier = aggregateIdentifier;
            SequenceNumber = sequenceNumber;
        }
        protected GenericDomainEventMessage(String type, String aggregateIdentifier, long sequenceNumber, IMessage @delegate, DateTime timestamp) : base(@delegate, timestamp)
        {
            Type = type;
            AggregateIdentifier = aggregateIdentifier;
            SequenceNumber = sequenceNumber;
        }

        public override IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericDomainEventMessage(Type, AggregateIdentifier, SequenceNumber, Delegate.WithMetaData(metaData), Timestamp);
        }

        public override IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (MetaData == null || metaData.IsEmpty() || MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericDomainEventMessage(Type, AggregateIdentifier, SequenceNumber, Delegate.AndMetaData(metaData), Timestamp);
        }

        protected override void DescribeTo(StringBuilder stringBuilder)
        {
            base.DescribeTo(stringBuilder);
            stringBuilder.Append('\'').Append(", aggregateType='")
                .Append(Type).Append('\'')
                .Append(", aggregateIdentifier='")
                .Append(AggregateIdentifier).Append('\'')
                .Append(", sequenceNumber=")
                .Append(SequenceNumber);
        }

        public string Type { get; }
        public long SequenceNumber { get; }
        public string AggregateIdentifier { get; }

        protected override string DescribeType => "GenericDomainEventMessage";
    }
}