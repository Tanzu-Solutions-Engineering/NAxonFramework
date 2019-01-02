using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public interface IAssociationResolver
    {
        void Validate(string assocationPropertyname, IMessageHandlingMember handler);
        object Resolve(string assocationPropertyname, IEventMessage message, IMessageHandlingMember handler);
    }
}