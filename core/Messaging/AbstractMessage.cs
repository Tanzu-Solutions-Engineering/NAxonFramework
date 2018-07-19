using System;
using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging
{
    public abstract class AbstractMessage<T> : IMessage<T>
    {
        private static readonly long _serialVersionUID = -5847906865361406657L;
        private readonly string _identifier;

        protected AbstractMessage(string identifier) 
        {
            this._identifier = identifier;
        }

        public virtual string Identifier { get; }
        public abstract MetaData MetaData { get; }
        public abstract Type PayloadType { get; }
        public abstract T Payload { get; }

        public virtual IMessage<T> WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (MetaData.Equals(metaData))
                return this;
            return WithMetaData(MetaData.From(metaData));
        }

        public virtual IMessage<T> AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (metaData.IsEmpty()) {
                return this;
            }
            return WithMetaData(MetaData.MergedWith(metaData));
        }

        object IMessage.Payload => Payload;
    }
}