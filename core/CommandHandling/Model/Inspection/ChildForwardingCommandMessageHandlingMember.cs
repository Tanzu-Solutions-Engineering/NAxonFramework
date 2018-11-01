using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class ChildForwardingCommandMessageHandlingMember : ICommandMessageHandlingMember
    {
        private readonly List<IMessageHandlingMember> _childHandlerInterceptors;
        private readonly IMessageHandlingMember _childHandler;
        private readonly Func<ICommandMessage, object, object> _childEntityResolver;
        private string _commandName;
        private bool _isFactoryHandler;

        public ChildForwardingCommandMessageHandlingMember(List<IMessageHandlingMember> childHandlerInterceptors,
        IMessageHandlingMember childHandler,
        Func<ICommandMessage, object, object> childEntityResolver)
        {
            _childHandlerInterceptors = childHandlerInterceptors;
            _childHandler = childHandler;
            _childEntityResolver = childEntityResolver;
            _commandName = _childHandler.Unwrap<ICommandMessageHandlingMember>()
                .Map(x => x.CommandName).OrElse(null);
            _isFactoryHandler = childHandler.Unwrap<ICommandMessageHandlingMember>()
                .Map(x => x.IsFactoryHandler).OrElse(false);
        }

        public string CommandName => _commandName;
        public string RoutingKey => null;
        public bool IsFactoryHandler => _isFactoryHandler;
        public Type PayloadType => _childHandler.PayloadType;
        public int Priority => int.MinValue;


        public bool CanHandle(IMessage message) => _childHandler.CanHandle(message);

        public object Handle(IMessage message, object target)
        {
            var childEntity = _childEntityResolver.Invoke((ICommandMessage)message, target);
            if (childEntity == null) 
            {
                throw new AggregateEntityNotFoundException(
                    "Aggregate cannot handle this command, as there is no entity instance to forward it to."
                );
            }
            var interceptors = _childHandlerInterceptors
                    .Where(chi => chi.CanHandle(message))
                    .OrderBy(x => x.Priority)
                    .Select(chi => new AnnotatedCommandHandlerInterceptor(chi, childEntity))
                    .ToList();

            object result;
            if (interceptors.IsEmpty()) 
            {
                result = _childHandler.Handle(message, childEntity);
            } 
            else 
            {
                result = new DefaultInterceptorChain(CurrentUnitOfWork.Get(),
                interceptors.GetEnumerator(),
                MessageHandler<ICommandMessage>.Create(m => _childHandler.Handle(message, childEntity))).Proceed();
            }
            return result;
        }

        public Optional<HT> Unwrap<HT>()
        {
            Type handlerType = typeof(HT);
            if (handlerType.IsInstanceOfType(this)) 
            {
                return Optional<HT>.Of(this);
            }
            return _childHandler.Unwrap<HT>();
        }

        public Optional<IDictionary<string, object>> AnnotationAttributes(Type attributeType)
        {
            return _childHandler.AnnotationAttributes(attributeType);
        }

        public bool HasAttribute(Type attributeType)
        {
            return _childHandler.HasAttribute(attributeType);
        }
    }
}