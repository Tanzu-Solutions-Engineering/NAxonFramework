using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public class ForwardNone<T> : IForwardingMode<T> where T : IMessage
    {
        public void Initialize(PropertyInfo property, IEntityModel childEntity)
        {
            
        }

        public IEnumerable<E> FilterCandidates<E>(T message, IEnumerable<E> candidates)
        {
            return Enumerable.Empty<E>();
        }
    }
}