using System;
using System.Collections.Generic;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IEntityModel
    {
        /**
* Get the identifier of the given {@code target} entity.
*
* @param target The entity instance
* @return The identifier of the given target entity
*/
        object GetIdentifier(object target);

        /**
   * Get the name of the routing key property on commands and events that provides the identifier that should be used
   * to target entities of this kind.
   *
   * @return The name of the routing key that holds the identifier used to target this sort of entity
   */
        string RoutingKey { get; }

        /**
   * Publish given event {@code message} on the given {@code target} entity.
   *
   * @param message The event message to publish
   * @param target  The target entity for the event
   */
        void Publish(IEventMessage message, object target);

        /**
   * Get a mapping of {@link MessageHandlingMember} to command name (obtained via {@link
   * org.axonframework.commandhandling.CommandMessage#getCommandName()}).
   *
   * @return Map of message handler to command name
   */
        IReadOnlyDictionary<String, IMessageHandlingMember> CommandHandlers { get; }

        /**
   * Gets a list of command handler interceptors for this entity.
   *
   * @return list of command handler interceptors
   */
        List<IMessageHandlingMember> CommandHandlerInterceptors { get; }

        /**
   * Get the {@link MessageHandlingMember} capable of handling commands with given {@code commandName} (see {@link
   * org.axonframework.commandhandling.CommandMessage#getCommandName()}). If the entity is not capable of handling
   * such commands an exception is raised.
   *
   * @param commandName The name of the command
   * @return The handler for the command
   * @throws NoHandlerForCommandException In case the entity is not capable of handling commands of given name
   */
        //TODO: check default interface impl
        IMessageHandlingMember CommandHandler(String commandName);

        /**
   * Get the EntityModel of an entity of type {@code childEntityType} in case it is the child of the modeled entity.
   *
   * @param childEntityType The class instance of the child entity type
   * @param <C>             the type of the child entity
   * @return An EntityModel for the child entity
   */
        IEntityModel ModelOf(Type childEntityType);

        /**
   * Returns the class this model describes
   *
   * @return the class this model describes
   */
        Type EntityClass { get; }
    }
}