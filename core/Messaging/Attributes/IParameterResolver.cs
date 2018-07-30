namespace NAxonFramework.Messaging.Attributes
{
    public interface IParameterResolver<T> : IParameterResolver{

        /**
         * Resolves the parameter value to use for the given {@code message}, or {@code null} if no suitable
         * parameter value can be resolved.
         *
         * @param message The message to resolve the value from
         * @return the parameter value for the handler
         */
        new T ResolveParameterValue(IMessage message);

        
    }
}