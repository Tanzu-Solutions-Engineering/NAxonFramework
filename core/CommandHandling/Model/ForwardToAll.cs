using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public class ForwardToAll<T> : IForwardingMode<T> where T : IMessage
    {
        public void Initialize(PropertyInfo property, IEntityModel childEntity)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<E> FilterCandidates<E>(T message, IEnumerable<E> candidates)
        {
            return candidates;
        }
    }
}