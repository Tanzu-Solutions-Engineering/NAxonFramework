using System;

namespace NAxonFramework.EventHandling.Saga
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SagaEventHandlerAttribute : Attribute
    {
        public SagaEventHandlerAttribute(string assocationProperty, string keyName = "", Type payloadType = null, Type assocationResolver = null)
        {
            AssocationProperty = assocationProperty;
            KeyName = keyName;
            PayloadType = payloadType ?? typeof(object);
            AssocationResolver = assocationResolver ?? typeof(PayloadAssociationResolver);
        }

        public string AssocationProperty { get; }
        public string KeyName { get; }
        public Type PayloadType { get; }
        public Type AssocationResolver { get; }
    }
}