using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga.MetaModel
{
    public interface ISagaModel
    {
        Optional<AssociationValue> ResolveAssociation(IEventMessage eventMessage);
        List<SagaMethodMessageHandlingMember> FindHandlerMethods(IEventMessage @event);
        bool HasHandlerMethod(IEventMessage eventMessage);
        ISagaMetaModelFactory ModelFactory { get; }
    }
}