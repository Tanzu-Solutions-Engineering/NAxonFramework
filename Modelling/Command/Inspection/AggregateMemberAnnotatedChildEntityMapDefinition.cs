using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AggregateMemberAnnotatedChildEntityMapDefinition : AbstractChildEntityDefinition
    {
        protected override bool IsFieldTypeSupported(PropertyInfo field)
        {
            return field.PropertyType.IsImplementing(typeof(IDictionary<,>));
        }

        protected override IEntityModel ExtractChildEntityModel(IEntityModel declaringEntity, IDictionary<string, object> attributes, PropertyInfo field)
        {
            var entityType = attributes.GetValueOrDefault("type") as Type;
            if (entityType == null)
            {
                entityType = field.ResolveGenericType(1).OrElseThrow(
                    () => new AxonConfigurationException($"Unable to resolve entity type of field {field.Name}. Please provide type explicitly in @AggregateMember annotation."));
                
            }

            return declaringEntity.ModelOf(entityType);
        }

        protected override object ResolveCommandTarget(ICommandMessage msg, object parent, PropertyInfo field, IEntityModel childEntityModel)
        {
            var commandHandlerRoutingKeys = ExtractCommandHandlerRoutingKeys(field, childEntityModel);
            var routingValue = commandHandlerRoutingKeys.GetValueOrDefault(msg.CommandName).GetValue<object>(msg.Payload);
            var fieldValue = field.GetValue(parent);
            return fieldValue == null ? null : fieldValue.GetOrDefault(routingValue);
        }

        protected override IEnumerable<object> ResolveEventTargets(IEventMessage message, object parentEntity, PropertyInfo field, IForwardingMode eventForwardingMode)
        {
            var fieldValue = field.GetValue(parentEntity);
            var valuesCollection = field.GetType().GetProperty(nameof(IDictionary.Values))?.GetValue(fieldValue) as IEnumerable;
            return valuesCollection == null ? Enumerable.Empty<object>() : eventForwardingMode.FilterCandidates(message,valuesCollection.Cast<object>());
        }
    }
}