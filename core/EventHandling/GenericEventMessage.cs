using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Serialization;

namespace NAxonFramework.EventHandling
{
    public class GenericEventMessage<T> : GenericEventMessage, IEventMessage<T>
    {
        public GenericEventMessage(T payload) : base(payload)
        {
        }

        public GenericEventMessage(T payload, IReadOnlyDictionary<string, object> metaData) : base(payload, metaData)
        {
        }

        public GenericEventMessage(string identifier, T payload, IReadOnlyDictionary<string, object> metaData, DateTime timestamp) : base(identifier, payload, metaData, timestamp)
        {
        }

        public GenericEventMessage(IMessage<T> @delegate, Func<DateTime> timestampSupplier) : base(@delegate, timestampSupplier)
        {
        }

        protected GenericEventMessage(IMessage<T> @delegate, DateTime timestamp) : base(@delegate, timestamp)
        {
        }

        public new T Payload => (T) base.Payload;
    }
    public class GenericEventMessage : MessageDecorator, IEventMessage
    {
        private static ConcurrentDictionary<Type, ActivatorDelegate<GenericEventMessage>> _activators = new ConcurrentDictionary<Type, ActivatorDelegate<GenericEventMessage>>();
        // todo creating generic based instances in non generic context. check if it makes sense to use everywhere
        public static GenericEventMessage Create(Type T, object payload)
        {
            return _activators.GetOrAdd(T, t => typeof(GenericEventMessage<>).MakeGenericType(t).CreateActivator().Cast<GenericEventMessage>())(payload);
        }
       
        
        
        private Func<DateTime> _timestampSupplier;
        
        public static Func<DateTime> Clock { get; } = () => DateTime.UtcNow;
        
        public static IEventMessage AsEventMessage(Object @event) 
        {
            if (@event is IEventMessage eventMessage) 
            {
                return eventMessage;
            } 
            else if (@event is IMessage message) 
            {
                return new GenericEventMessage(message, () => Clock());
            }
            return new GenericEventMessage(new GenericMessage(@event), Clock());
        }

        public GenericEventMessage(object payload) : this(payload, MetaData.EmptyInstance)
        {
        }
        public GenericEventMessage(object payload, IReadOnlyDictionary<String, object> metaData) 
            : this(new GenericMessage(payload, metaData), Clock())
        {
        }
        
        public GenericEventMessage(string identifier, object payload, IReadOnlyDictionary<String, object> metaData, DateTime timestamp) 
            : this(new GenericMessage(identifier, payload, metaData), timestamp)
        {
        }

        public GenericEventMessage(IMessage @delegate, Func<DateTime> timestampSupplier) : base(@delegate)
        {
            _timestampSupplier = timestampSupplier;
        }
        protected GenericEventMessage(IMessage @delegate, DateTime timestamp) : this(@delegate, CachingSupplier<DateTime>.Of(timestamp).Get)
        {
        }

        public DateTime Timestamp => _timestampSupplier();


        public override IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (this.MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericEventMessage(this.Delegate.WithMetaData(metaData), _timestampSupplier);
        }

        
        public override IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (metaData == null || !metaData.Any() || this.MetaData.Equals(metaData))
            {
                return this;
            }
            return new GenericEventMessage(this.Delegate.AndMetaData(metaData), _timestampSupplier);
        }

        
        protected override void DescribeTo(StringBuilder stringBuilder)
        {
            
            base.DescribeTo(stringBuilder);
            stringBuilder.Append(", timestamp='").Append(Timestamp);
        }


        protected override string DescribeType => "GenericEventMessage";
    }


}