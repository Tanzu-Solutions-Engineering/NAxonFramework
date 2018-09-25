using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.EventHandling;
using NAxonFramework.Common;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AggregateMemberAnnotatedChildEntityDefinition : AbstractChildEntityDefinition
    {
        protected override bool IsFieldTypeSupported(PropertyInfo field)
        {
            IDictionary<string, string> s;
            return !field.PropertyType.IsImplementing<IEnumerable>();
        }

        protected override IEntityModel ExtractChildEntityModel(IEntityModel declaringEntity, IDictionary<string, object> attributes, PropertyInfo field)
        {
            return declaringEntity.ModelOf(field.PropertyType);
        }

        protected override object ResolveCommandTarget(ICommandMessage msg, object parent, PropertyInfo field, IEntityModel childEntityModel)
        {
            return field.GetValue(parent);
        }

        protected override IEnumerable<object> ResolveEventTargets(IEventMessage message, object parentEntity, PropertyInfo field, IForwardingMode eventForwardingMode)
        {
            var fieldVal = field.GetValue(parentEntity);
            return fieldVal == null ? Enumerable.Empty<object>() : eventForwardingMode.FilterCandidates(message, (IEnumerable<object>)(fieldVal));
        }
    }
}