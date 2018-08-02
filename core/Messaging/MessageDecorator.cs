using System;
using System.Collections.Generic;
using System.Text;
using NAxonFramework.CommandHandling;
using NAxonFramework.Serialization;

namespace NAxonFramework.Messaging
{
    public abstract class MessageDecorator<T> : MessageDecorator, IMessage<T>
    {
        protected MessageDecorator(IMessage @delegate) : base(@delegate)
        {
        }

        public new T Payload => (T)base.Payload;
    }
    
    public abstract class MessageDecorator : IMessage, ISerializationAware
    {
        private static readonly long serialVersionUID = 3969631713723578521L;

        [NonSerialized]
        private SerializedObjectHolder _serializedObjectHolder;

        protected IMessage Delegate { get; }

        protected MessageDecorator(IMessage @delegate) 
        {
            this.Delegate = @delegate;
        }

        public String Identifier => Delegate.Identifier;

        public MetaData MetaData => Delegate.MetaData;
        public object Payload => Delegate.Payload;
        public abstract IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData);
        public abstract IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData);
        public Type PayloadType => Delegate.PayloadType;
        object IMessage.Payload => Payload;

        public virtual ISerializedObject<S> SerializePayload<S>(ISerializer serializer) 
        {
            //Type expectedRepresentation
            if (Delegate is ISerializationAware) {
                return ((ISerializationAware) Delegate).SerializePayload<S>(serializer);
            }
            return GetSerializedObjectHolder().SerializePayload<S>(serializer);
        }
        public virtual  ISerializedObject<S> SerializeMetaData<S>(ISerializer serializer) 
        {
            if (Delegate is ISerializationAware) {
                return ((ISerializationAware) Delegate).SerializeMetaData<S>(serializer);
            }
            return GetSerializedObjectHolder().SerializeMetaData<S>(serializer);
        }
        private SerializedObjectHolder GetSerializedObjectHolder() 
        {
            if (_serializedObjectHolder == null) {
                _serializedObjectHolder = new SerializedObjectHolder(Delegate);
            }
            return _serializedObjectHolder;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder()
                .Append(DescribeType)
                .Append("{");
            DescribeTo(sb);
            return sb.Append("}")
                .ToString();
        }

        protected virtual string DescribeType => this.GetType().Name;
        protected virtual void DescribeTo(StringBuilder stringBuilder)
        {
            stringBuilder.Append("payload={")
                .Append(Payload)
                .Append('}')
                .Append(", metadata={")
                .Append(MetaData)
                .Append('}')
                .Append(", messageIdentifier='")
                .Append(Identifier)
                .Append('\'');
        }
    }
}