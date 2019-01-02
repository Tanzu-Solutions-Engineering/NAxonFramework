using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AnnotatedChildEntity : IChildEntity
    {
        private readonly IEntityModel _entityModel;
        private readonly Func<IEventMessage, object, IEnumerable<object>> _eventTargetResolver;
        private readonly List<IMessageHandlingMember> _commandHandlers;

        public AnnotatedChildEntity(IEntityModel entityModel, bool forwardCommands, 
            Func<ICommandMessage, object, object> commandTargetResolver,
            Func<IEventMessage, object, IEnumerable<object>> eventTargetResolver)
        {
            _entityModel = entityModel;
            _eventTargetResolver = eventTargetResolver;
            _commandHandlers = new List<IMessageHandlingMember>();
            if (forwardCommands)
            {
                entityModel.CommandHandlers
                    .Where(eh => eh.Unwrap<ICommandMessageHandlingMember>().IsPresent)
                    .ForEach(childHandler => _commandHandlers
                    .Add(new ChildForwardingCommandMessageHandlingMember(
                        entityModel.CommandHandlerInterceptors,
                        childHandler,
                        commandTargetResolver)));
            }
        }

        public void Publish(IEventMessage msg, object declaringInstance)
        {
            _eventTargetResolver.Invoke(msg, declaringInstance)
                .ToList()
                .ForEach(target => _entityModel.Publish(msg, target));
        }

        public IList<IMessageHandlingMember> CommandHandlers => _commandHandlers;
    }
}