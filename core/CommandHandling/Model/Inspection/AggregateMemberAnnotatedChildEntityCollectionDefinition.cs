using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AggregateMemberAnnotatedChildEntityCollectionDefinition : AbstractChildEntityDefinition
    {
        protected override bool IsFieldTypeSupported(PropertyInfo field)
        {
            return field.PropertyType.IsImplementing<IEnumerable>();
        }

        protected override IEntityModel ExtractChildEntityModel(IEntityModel declaringEntity, IDictionary<string, object> attributes, PropertyInfo field)
        {
            var entityType = (Type)attributes["type"];
            if (entityType == null) 
            {
                entityType = field.ResolveGenericType(0).OrElseThrow(
                    () => new AxonConfigurationException(
                        $"Unable to resolve entity type of field {field}. Please provide type explicitly in @AggregateMember annotation."
                ));
            }

            return declaringEntity.ModelOf(entityType);

        }

        protected override object ResolveCommandTarget(ICommandMessage msg, object parent, PropertyInfo field, IEntityModel childEntityModel)
        {
            var commandHandlerRoutingKeys = ExtractCommandHandlerRoutingKeys(field, childEntityModel);

            var routingValue = commandHandlerRoutingKeys.GetValueOrDefault(msg.CommandName).GetValue<object>(msg.Payload);
            var enumerable = (IEnumerable) field.GetValue(parent);
            return enumerable
                .Cast<object>()
                .FirstOrDefault(i => Equals(routingValue, childEntityModel.GetIdentifier(i)));
        }

        protected override IEnumerable<object> ResolveEventTargets(IEventMessage message, object parentEntity, PropertyInfo field, IForwardingMode eventForwardingMode)
        {
            var fieldValue = (IEnumerable)field.GetValue(parentEntity);
            return fieldValue == null
                ? Enumerable.Empty<object>()
                : eventForwardingMode.FilterCandidates(message, fieldValue.Cast<object>());

        }
    }
}