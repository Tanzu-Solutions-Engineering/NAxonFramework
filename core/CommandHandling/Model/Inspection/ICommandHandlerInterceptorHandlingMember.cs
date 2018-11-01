namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface ICommandHandlerInterceptorHandlingMember
    {
        bool ShouldInvokeInterceptorChain();
        
    }
}