using System;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public class PayloadAssociationResolver : IAssociationResolver
    {
        private Dictionary<string, PropertyInfo> _propertyMap = new Dictionary<string, PropertyInfo>();

        public void Validate(string assocationPropertyname, IMessageHandlingMember handler)
        {
            
        }

        public object Resolve(string assocationPropertyname, IEventMessage message, IMessageHandlingMember handler)
        {
            throw new NotImplementedException();
        }

        private PropertyInfo GetProperty(String associationPropertyName, IMessageHandlingMember handler)
        {
            return _propertyMap.ComputeIfAbsent(handler.PayloadType.FullName + associationPropertyName, k => CreateProperty(associationPropertyName, handler));
        }

        private PropertyInfo CreateProperty(string associationPropertyName, IMessageHandlingMember handler)
        {
            var associationProperty = PropertyAccessStrategy.GetProperty(handler.PayloadType, associationPropertyName);
            if (associationProperty == null)
            {
                var handlerName = handler.Unwrap<MethodInfo>().Map(x => x.ToString()).OrElse("unknown");
                throw new AxonConfigurationException($"SagaEventHandler {handlerName} defines a property {associationPropertyName} that is not defined on the Event it declares to handle ({handler.PayloadType.Name})");
            }

            return associationProperty;
        }
    }
}