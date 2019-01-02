using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public class ForwardNone : IForwardingMode
    {
        public void Initialize(PropertyInfo property, IEntityModel childEntity)
        {
            
        }

        public IEnumerable<E> FilterCandidates<E>(IMessage message, IEnumerable<E> candidates)
        {
            return Enumerable.Empty<E>();
        }
    }
}