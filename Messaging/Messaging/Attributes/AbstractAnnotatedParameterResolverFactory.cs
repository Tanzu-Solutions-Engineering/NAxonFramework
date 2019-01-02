using System;
using System.Reflection;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    public abstract class AbstractAnnotatedParameterResolverFactory<A,P> : IParameterResolverFactory where A : Attribute
    {
        private readonly Type _declaredParameterType;
        private Type _annotationType;

        protected AbstractAnnotatedParameterResolverFactory()
        {
            _annotationType = typeof(A);
            _declaredParameterType = typeof(P);
        }

        protected abstract IParameterResolver<P> GetResolver();
        
        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            if (parameters[parameterIndex].HasAttribute<A>())
            {
                var parameterType = parameters[parameterIndex].ParameterType;
                if (parameterType.IsAssignableFrom(_declaredParameterType))
                    return GetResolver();
            }

            return null;

        }
    }
}