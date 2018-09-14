using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IForwardingMode<T> where T : IMessage
    {
        void Initialize(PropertyInfo property, IEntityModel childEntity);
        IEnumerable<E> FilterCandidates<E>(T message, IEnumerable<E> candidates);
    }
}