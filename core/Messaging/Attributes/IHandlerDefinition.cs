using System;
using System.Reflection;

namespace NAxonFramework.Messaging.Attributes
{
    public interface IHandlerDefinition
    {
        IMessageHandlingMember<T> CreateHandler<T>(MethodBase executable, IParameterResolverFactory parameterResolverFactory);
        IMessageHandlingMember CreateHandler(Type type, MethodBase executable, IParameterResolverFactory parameterResolverFactory);
    }
}