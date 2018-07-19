using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAxonFramework.Messaging;
using NAxonFramework.Serialization;

namespace NAxonFramework.EventHandling
{
    public class GenericEventMessage<T> : MessageDecorator<T>, IEventMessage<T>
    {
        private static long _serialVersionUID = -8296350547944518544L;
        private Func<DateTime> _timestampSupplier;
        
        public static Func<DateTime> Clock { get; } = () => DateTime.UtcNow;
        
        public static IEventMessage<T> AsEventMessage(Object @event) 
        {
            if (@event is IEventMessage<T> eventMessage) 
            {
                return eventMessage;
            } 
            else if (@event is IMessage<T> message) 
            {
                return new GenericEventMessage<T>(message, () => Clock());
            }
            return new GenericEventMessage<T>(new GenericMessage<T>((T)@event), Clock());
        }

        public GenericEventMessage(T payload) : this(payload, MetaData.EmptyInstance)
        {
        }
        public GenericEventMessage(T payload, IReadOnlyDictionary<String, object> metaData) : this(new GenericMessage<T>(payload, metaData), Clock())
        {
        }
        
        public GenericEventMessage(string identifier, T payload, IReadOnlyDictionary<String, object> metaData, DateTime timestamp) : this(new GenericMessage<T>(identifier, payload, metaData), timestamp)
        {
        }

        public GenericEventMessage(IMessage<T> @delegate, Func<DateTime> timestampSupplier) : base(@delegate)
        {
            _timestampSupplier = timestampSupplier;
        }
        protected GenericEventMessage(IMessage<T> @delegate, DateTime timestamp) : this(@delegate, CachingSupplier<DateTime>.Of(timestamp).Get)
        {
        }

        public DateTime Timestamp => _timestampSupplier();


        public override IMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (this.MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericEventMessage<T>(this.Delegate.WithMetaData(metaData), _timestampSupplier);
        }

        
        public override IMessage<T> AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (metaData == null || !metaData.Any() || this.MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericEventMessage<T>(this.Delegate.AndMetaData(metaData), _timestampSupplier);
        }

        
        protected override void DescribeTo(StringBuilder stringBuilder)
        {
            
            base.DescribeTo(stringBuilder);
            stringBuilder.Append(", timestamp='").Append(Timestamp);
        }


        protected override string DescribeType => "GenericEventMessage";
    }


}