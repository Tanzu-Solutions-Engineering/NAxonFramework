using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Common;
using NAxonFramework.Common.Attributes;
using NAxonFramework.Messaging;

namespace NAxonFramework.CommandHandling.Model
{
    public class ForwardMatchingInstances : IForwardingMode
    {
        private string _routingKey;
        private IEntityModel _childEntity;
        public void Initialize(PropertyInfo property, IEntityModel childEntity)
        {
            _childEntity = childEntity;
            _routingKey = AnnotationUtils.FindAnnotationAttributes(property, typeof(AggregateMemberAttribute))
                .Map(map => (string) map.GetValueOrDefault("routingKey"))
                .Filter(key => !string.IsNullOrEmpty(key))
                .OrElse(childEntity.RoutingKey);
        }

        public IEnumerable<E> FilterCandidates<E>(IMessage message, IEnumerable<E> candidates)
        {
            var routingProperty = message.GetType().GetProperty(_routingKey);
            if (routingProperty == null)
                return Enumerable.Empty<E>();
            var routingValue = routingProperty.GetValue(message.Payload);
            return candidates.Where(candidate => MatchesInstance(candidate, routingValue));
        }

        private bool MatchesInstance<E>(E candidate, object routingValue)
        {
            var identifier = _childEntity.GetIdentifier(candidate);
            return Equals(routingValue, identifier);
        }
    }
}