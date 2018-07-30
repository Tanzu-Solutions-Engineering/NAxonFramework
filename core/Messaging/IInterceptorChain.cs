namespace NAxonFramework.Messaging
{
    public interface IInterceptorChain
    {
        
        object Proceed();
    }
}