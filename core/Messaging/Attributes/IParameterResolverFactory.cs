using System.Reflection;

namespace NAxonFramework.Messaging.Attributes
{
    public interface IParameterResolverFactory
    {
        IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex);
    }
}