using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AnnotatedCommandHandlerInterceptor : IMessageHandlerInterceptor
    {
        private readonly IMessageHandlingMember _delegate;
        private readonly object _target;

        public AnnotatedCommandHandlerInterceptor(IMessageHandlingMember @delegate, object target)
        {
            _delegate = @delegate;
            _target = target;
        }

        public object Handle(IUnitOfWork unitOfWork, IInterceptorChain interceptorChain)
        {
            InterceptorChainParameterResolverFactory.Initialize(interceptorChain);

            var result = _delegate.Handle(unitOfWork.Message, _target);

            if (_delegate.Unwrap<ICommandHandlerInterceptorHandlingMember>()
                .Map(x => x.ShouldInvokeInterceptorChain()).OrElse(false))
            {
                result = interceptorChain.Proceed();
            }

            return result;
        }
    }
}