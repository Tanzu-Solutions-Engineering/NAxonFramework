using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public class SagaMethodMessageHandlingMember : WrappedMessageHandlingMember
    {
        private readonly IMessageHandlingMember _delegate;
        public SagaCreationPolicy CreationPolicy { get; }
        private readonly string _associationKey;
        private readonly string _associationPropertyName;
        private readonly IAssociationResolver _associationResolver;
        public bool IsEndingHandler { get; }

        public SagaMethodMessageHandlingMember(IMessageHandlingMember @delegate, SagaCreationPolicy creationPolicy,
            string associationKey, string associationPropertyName,
            IAssociationResolver associationResolver, bool isEndingHandler) : base(@delegate)
        {
            _delegate = @delegate;
            CreationPolicy = creationPolicy;
            _associationKey = associationKey;
            _associationPropertyName = associationPropertyName;
            _associationResolver = associationResolver;
            IsEndingHandler = isEndingHandler;
        }
        
        public AssociationValue GetAssociationValue(IEventMessage eventMessage) 
        {
            if (_associationResolver == null) 
            {
                return null;
            }
            var associationValue = _associationResolver.Resolve(_associationPropertyName, eventMessage, this);
            return associationValue == null ? null : new AssociationValue(_associationKey, associationValue.ToString());
        }

        public override object Handle(IMessage message, object target)
        {
            return _delegate.Handle(message, target);
        }
        
    }
}