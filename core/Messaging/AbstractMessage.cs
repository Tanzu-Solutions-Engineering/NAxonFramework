using System;
using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging
{


    public abstract class AbstractMessage<T> : AbstractMessage, IMessage<T>
    {
        protected AbstractMessage(string identifier) : base(identifier)
        {
        }

        public new T Payload => (T) base.Payload;
    }
    public abstract class AbstractMessage : IMessage
    {
        private static readonly long _serialVersionUID = -5847906865361406657L;
        private readonly string _identifier;

        protected AbstractMessage(string identifier) 
        {
            this._identifier = identifier;
        }

        public virtual string Identifier { get; }
        public virtual MetaData MetaData { get; }
        public virtual Type PayloadType { get; }
        public virtual object Payload { get; }

        public virtual IMessage WithMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (MetaData.Equals(metaData))
                return this;
            return WithMetaData(MetaData.From(metaData));
        }

        public virtual IMessage AndMetaData(IReadOnlyDictionary<string, object> metaData)
        {
            if (metaData.IsEmpty()) {
                return this;
            }
            return WithMetaData(MetaData.MergedWith(metaData));
        }

        object IMessage.Payload => Payload;
    }
}