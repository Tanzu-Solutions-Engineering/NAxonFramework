using System;
using System.Collections.Generic;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.Messaging
{
    public class DefaultInterceptorChain : IInterceptorChain
    {
        private readonly IMessageHandler _handler;
        private readonly IEnumerator<IMessageHandlerInterceptor> _chain;
        private readonly IUnitOfWork _unitOfWork;


        public DefaultInterceptorChain(IUnitOfWork unitOfWork, IEnumerator<IMessageHandlerInterceptor> chain, IMessageHandler handler)
        {
            _handler = handler;
            _chain = chain;
            _unitOfWork = unitOfWork;
        }

        public object Proceed()
        {
            if (_chain.MoveNext())
            {
                return _chain.Current.Handle(_unitOfWork, this);
            }
            else
            {
                return _handler.Handle(_unitOfWork.Message);
            }
        }
    }
}