using System;

namespace NAxonFramework.Messaging.Attributes
{
    public interface IParameterResolver
    {
        /**
         * Indicates whether this resolver is capable of providing a value for the given {@code message}.
         *
         * @param message The message to evaluate
         * @return {@code true} if this resolver can provide a value for the message, otherwise {@code false}
         */
        bool Matches(IMessage message);

        /**
         * Returns the class of the payload that is supported by this resolver. Defaults to the {@link Object} class
         * indicating that the payload type is irrelevant for this resolver.
         *
         * @return The class of the payload that is supported by this resolver
         */
        Type SupportedPayloadType { get; } 
        object ResolveParameterValue(IMessage message);
    }
}