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
        private readonly Dictionary<string, IMessageHandlingMember> _commandHandlers;

        public AnnotatedChildEntity(IEntityModel entityModel, bool forwardCommands, 
            Func<ICommandMessage, object, object> commandTargetResolver,
            Func<IEventMessage, object, IEnumerable<object>> eventTargetResolver)
        {
            _entityModel = entityModel;
            _eventTargetResolver = eventTargetResolver;
            _commandHandlers = new Dictionary<string, IMessageHandlingMember>();
            if (forwardCommands)
            {
                _entityModel.CommandHandlers.ForEach(kv => 
                    _commandHandlers.Add(kv.Key, 
                        new ChildForwardingCommandMessageHandlingMember(entityModel.CommandHandlerInterceptors, kv.Value, commandTargetResolver)));
            }
        }

        public void Publish(IEventMessage msg, object declaringInstance)
        {
            _eventTargetResolver.Invoke(msg, declaringInstance)
                .ToList()
                .ForEach(target => _entityModel.Publish(msg, target));
        }

        public IReadOnlyDictionary<string, IMessageHandlingMember> CommandHandlers => _commandHandlers;
    }
}