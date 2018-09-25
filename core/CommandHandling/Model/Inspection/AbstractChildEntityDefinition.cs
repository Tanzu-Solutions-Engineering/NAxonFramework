using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using NAxonFramework.Common;
using NAxonFramework.Common.Property;
using NAxonFramework.EventHandling;
using static NAxonFramework.Common.Attributes.AnnotationUtils;
using static NAxonFramework.Common.Property.PropertyAccessStrategy;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public abstract class AbstractChildEntityDefinition : IChildEntityDefinition
    {
        public Optional<IChildEntity> CreateChildDefinition(PropertyInfo field, IEntityModel declaringEntity)
        {
            var attributes = FindAnnotationAttributes(field, typeof(AggregateMemberAttribute)).OrElse(null);
            if (attributes == null || !IsFieldTypeSupported(field)) 
            {
                return Optional<IChildEntity>.Empty;
            }

            var childEntityModel = ExtractChildEntityModel(declaringEntity, attributes, field);

            var eventForwardingMode = InstantiateForwardingMode(
                field, childEntityModel, attributes["eventForwardingMode"]
                );

            return Optional<IChildEntity>.Of(new AnnotatedChildEntity(
                childEntityModel,
                (Boolean) attributes["forwardCommands"],
                (msg, parent) -> resolveCommandTarget(msg, parent, field, childEntityModel),
                (msg, parent) -> resolveEventTargets(msg, parent, field, eventForwardingMode)
            ));

        }
        protected abstract bool IsFieldTypeSupported(PropertyInfo field);
        protected abstract IEntityModel ExtractChildEntityModel(IEntityModel declaringEntity,
            IDictionary<String, Object> attributes,
            PropertyInfo field);

        private IForwardingMode InstantiateForwardingMode(PropertyInfo field,
            IEntityModel childEntityModel,
            Type forwardingModeClass) {
            IForwardingMode forwardingMode;
            try 
            {
                forwardingMode = (IForwardingMode)Activator.CreateInstance(forwardingModeClass);
                forwardingMode.Initialize(field, childEntityModel);
            } 
            catch (Exception ex) when (ex is MissingMethodException) 
            {
                throw new AxonConfigurationException($"Failed to instantiate ForwardingMode of type {forwardingModeClass}.");
            }

            return forwardingMode;
        }
        protected abstract object ResolveCommandTarget(ICommandMessage msg,
        object parent,
            PropertyInfo field,
        IEntityModel childEntityModel);
        
        protected Dictionary<string, IProperty<object>> ExtractCommandHandlerRoutingKeys(PropertyInfo field,
            IEntityModel childEntityModel)
        {
            return childEntityModel.CommandHandlers
                .Values
                .Select(commandHandler => commandHandler.Unwrap<ICommandMessageHandlingMember>().OrElse(null))
                .Where(x => x != null)
                .ToDictionary(x => x.CommandName, commandHandler => ExtractCommandHandlerRoutingKey(childEntityModel, commandHandler, field));
        }
        
        private IProperty<Object> ExtractCommandHandlerRoutingKey(IEntityModel childEntityModel,
            ICommandMessageHandlingMember commandHandler,
            PropertyInfo field) {
            var routingKey = commandHandler.RoutingKey.GetOrDefault(childEntityModel.RoutingKey);

            var property = GetProperty<object>(commandHandler.PayloadType, routingKey);

            if (property == null) {
                throw new AxonConfigurationException(
                    $"Command of type {commandHandler.PayloadType} doesn't have a property matching the routing key {routingKey} necessary to route through field {field}",
                );
            }
            return property;
        }
        protected abstract IEnumerable<object> ResolveEventTargets(IEventMessage message,
            object parentEntity,
            PropertyInfo field,
            IForwardingMode eventForwardingMode);
    }
}