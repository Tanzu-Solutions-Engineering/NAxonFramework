namespace NAxonFramework.Messaging
{
    public interface InterceptorChain
    {
        object Proceed();
    }
}