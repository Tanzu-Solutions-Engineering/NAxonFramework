using System;
using System.Reflection;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling
{
    public class CurrentUnitOfWorkParameterResolverFactory : IParameterResolverFactory, IParameterResolver
    {
        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            if (parameters[parameterIndex].ParameterType == typeof(IUnitOfWork))
            {
                return this;
            }

            return null;
        }

        public object ResolveParameterValue(IMessage message)
        {
            if (!CurrentUnitOfWork.IsStarted)
            {
                return null;
            }

            return CurrentUnitOfWork.Get();
        }

        public bool Matches(IMessage message) => true;

        public Type SupportedPayloadType => typeof(object);
    }
}