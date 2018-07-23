namespace NAxonFramework.Messaging.UnitOfWork
{
    public interface IMessageHandlerInterceptor
    {
        object Handle(IUnitOfWork unitOfWork, InterceptorChain interceptorChain);
    }
}