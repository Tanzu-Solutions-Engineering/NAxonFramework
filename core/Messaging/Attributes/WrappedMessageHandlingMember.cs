using System;
using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    public class WrappedMessageHandlingMember : IMessageHandlingMember
    {
        private readonly IMessageHandlingMember _delegate;

        public WrappedMessageHandlingMember(IMessageHandlingMember @delegate)
        {
            _delegate = @delegate;
        }

        public Type PayloadType => _delegate.PayloadType;
        public int Priority => _delegate.Priority;
        public virtual bool CanHandle(IMessage message) => _delegate.CanHandle(message);

        public virtual object Handle(IMessage message, object target) => _delegate.Handle(message, target);

        public Optional<HT> Unwrap<HT>()
        {
            if (typeof(HT) == this.GetType())
            {
                // todo: find a way to bypass this reflection hack
                return (Optional<HT>)typeof(Optional<>).MakeGenericType(typeof(HT)).GetMethod(nameof(Optional<HT>.Of)).Invoke(null, new[]{this});
                //return Optional<HT>.Of(this as HT);
            }
            return _delegate.Unwrap<HT>();
        }
        public bool HasAttribute(Type attributeType) => _delegate.HasAttribute(attributeType);

        public Optional<IDictionary<string, object>> AnnotationAttributes(Type attributeType) => _delegate.AnnotationAttributes(attributeType);
    }
}