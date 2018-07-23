using System;
using System.Collections.Generic;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    
 /**
  * Interface describing a handler for specific messages targeting entities of a specific type.
  *
  * @param <T> The type of entity to which the message handler will delegate the actual handling of the message
  */
  public interface IMessageHandlingMember<T> : IMessageHandlingMember
  {
 
   /**
    * Handles the given {@code message} by invoking the appropriate method on given {@code target}. This may result in
    * an exception if the given target is not capable of handling the message or if an exception is thrown during
    * invocation of the method.
    *
    * @param message The message to handle
    * @param target  The target to handle the message
    * @return The message handling result in case the invocation was successful
    * @throws Exception when there was a problem that prevented invocation of the method or if an exception was thrown
    *                   from the invoked method
    */
   object Handle(IMessage message, T target);
  }

 public interface IMessageHandlingMember
 {
  /**
    * Returns the payload type of messages that can be processed by this handler.
    *
    * @return The payload type of messages expected by this handler
    */
   Type PayloadType { get; }
 
   /**
    * Returns a number representing the priority of this handler over other handlers capable of processing the same
    * message.
    * <p>
    * In general, a handler with a higher priority will receive the message before (or instead of) handlers with a
    * lower priority. However, the priority value may not be the only indicator that is used to determine the order of
    * invocation. For instance, a message processor may decide to ignore the priority value if one message handler is
    * a more specific handler of the message than another handler.
    *
    * @return Number indicating the priority of this handler over other handlers
    */
   int Priority { get; }
 
   /**
    * Checks if this handler is capable of handling the given {@code message}.
    *
    * @param message The message that is to be handled
    * @return {@code true} if the handler is capable of handling the message, {@code false} otherwise
    */
   bool CanHandle(IMessage message);
 
   /**
    * Handles the given {@code message} by invoking the appropriate method on given {@code target}. This may result in
    * an exception if the given target is not capable of handling the message or if an exception is thrown during
    * invocation of the method.
    *
    * @param message The message to handle
    * @param target  The target to handle the message
    * @return The message handling result in case the invocation was successful
    * @throws Exception when there was a problem that prevented invocation of the method or if an exception was thrown
    *                   from the invoked method
    */
   Object Handle(IMessage message, object target);
 
    /**
     * Returns the wrapped handler object if its type is an instance of the given {@code handlerType}. For instance, if
     * this method is invoked with {@link java.lang.reflect.Executable} and the message is handled by a method of the
     * target entity, then this method will return the method handle as a {@link java.lang.reflect.Method}.
     *
     * @param handlerType The expected type of the wrapped handler
     * @param <HT>        The wrapped handler type
     * @return An Optional containing the wrapped handler object or an empty Optional if the handler is not an instance
     * of the given handlerType
     */
   //TODO: possibly not needed
  Optional<HT> Unwrap<HT>();// where HT : struct;
 
   /**
    * Checks whether the method of the target entity contains the given {@code annotationType}.
    *
    * @param annotationType Annotation to check for on the target method
    * @return {@code true} if the annotation is present on the target method, {@code false} otherwise
    */
   bool HasAttribute(Type attributeType);
 
   /**
    * Get the attributes of an annotation of given {@code annotationType} on the method of the target entity. If the
    * annotation is present on the target method an Optional is returned containing the properties mapped by their
    * name. If the annotation is not present an empty Optional is returned.
    *
    * @param annotationType The annotation to check for on the target method
    * @return An optional containing a map of the properties of the annotation, or an empty optional if the annotation
    * is missing on the method
    */
   //TODO: Rename to match .NET scheme
   IDictionary<String, Object> AnnotationAttributes(Type attributeType);
   
 }
}