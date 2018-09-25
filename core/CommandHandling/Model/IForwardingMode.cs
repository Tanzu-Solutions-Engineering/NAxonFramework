using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IForwardingMode
    {
        void Initialize(PropertyInfo property, IEntityModel childEntity);
        IEnumerable<E> FilterCandidates<E>(IMessage message, IEnumerable<E> candidates);
    }
    
}