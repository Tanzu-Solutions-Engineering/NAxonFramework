using System;
using System.Collections.Generic;
using System.Threading;
using NAxonFramework.Common;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{
    public class GenericMessage<T> : AbstractMessage<T>
    {
        private const long serialVersionUID = 7937214711724527316L;


        public GenericMessage(T payload) : this(payload, MetaData.EmptyInstance)
        {
            
        }

        public GenericMessage(T payload, IReadOnlyDictionary<string, object> metadata) : this(IdentifierFactory.Instance.GenerateIdentifier(), payload, 
            CurrentUnitOfWork.CorrelationData.MergedWith(MetaData.From(metadata)))
        {
            
            
        }

        public GenericMessage(string identifier, T payload, IReadOnlyDictionary<string, object> metaData) : base(identifier)
        {
            Payload = payload;
            MetaData = MetaData.From(metaData);
            PayloadType = typeof(T);
        }

        private GenericMessage(GenericMessage<T> original, MetaData metaData, Type type) : base(original.Identifier)
        {
            Payload = original.Payload;
            MetaData = MetaData.From(metaData);
            PayloadType = typeof(T);
        }

        public override MetaData MetaData { get; }
        public override Type PayloadType { get; }
        public override T Payload { get; }

        public override IMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData) => new GenericMessage<T>(this, new MetaData(metaData), typeof(T));
        
    }
}