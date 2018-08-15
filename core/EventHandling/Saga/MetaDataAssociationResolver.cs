using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public class MetaDataAssociationResolver : IAssociationResolver
    {
        public void Validate(string assocationPropertyname, IMessageHandlingMember handler)
        {
            
        }

        public object Resolve(string assocationPropertyname, IEventMessage message, IMessageHandlingMember handler)
        {
            return message.MetaData.GetValueOrDefault(assocationPropertyname);
        }
    }
}