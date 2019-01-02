using System;
using System.Collections.Generic;
using System.Threading;
using NAxonFramework.Common;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{

    public class GenericMessage<T> : GenericMessage, IMessage<T>
    {
        public GenericMessage(T payload) : base(payload)
        {
        }

        public GenericMessage(T payload, IReadOnlyDictionary<string, object> metadata) : base(payload, metadata)
        {
        }

        public GenericMessage(string identifier, T payload, IReadOnlyDictionary<string, object> metaData) : base(identifier, payload, metaData)
        {
        }

        public T Payload => (T) base.Payload;
    }
    public class GenericMessage : AbstractMessage
    {
        private const long serialVersionUID = 7937214711724527316L;


        public GenericMessage(object payload) : this(payload, MetaData.EmptyInstance)
        {
            
        }

        public GenericMessage(object payload, IReadOnlyDictionary<string, object> metadata) 
            : this(IdentifierFactory.Instance.GenerateIdentifier(), payload, 
            CurrentUnitOfWork.CorrelationData.MergedWith(MetaData.From(metadata)))
        {
            
            
        }
        public GenericMessage(Type declaredPayloadType, object payload, IReadOnlyDictionary<string, object> metadata) 
            : this(IdentifierFactory.Instance.GenerateIdentifier(), declaredPayloadType, payload, 
            CurrentUnitOfWork.CorrelationData.MergedWith(MetaData.From(metadata)))
        {
            
            
        }

        public GenericMessage(string identifier, object payload, IReadOnlyDictionary<string, object> metaData) : 
            this(identifier, payload.GetType(), payload, metaData)
        {

        }

        private GenericMessage(string identifier, Type declaredPayloadType, object payload, IReadOnlyDictionary<string, object> metaData) : base(identifier)
        {
            Payload = payload;
            MetaData = MetaData.From(metaData);
            PayloadType = declaredPayloadType;
        }

        private GenericMessage(GenericMessage original, MetaData metaData) : base(original.Identifier)
        {
            Payload = original.Payload;
            PayloadType = original.PayloadType;
            MetaData = MetaData.From(metaData);
        }

        public override MetaData MetaData { get; }
        public override Type PayloadType { get; }
        public override object Payload { get; }

        public override IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData) 
            => new GenericMessage(this, new MetaData(metaData));
        
    }
}