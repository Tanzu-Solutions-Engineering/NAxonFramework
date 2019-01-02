namespace NAxonFramework.EventHandling
{
    public interface IErrorHandler
    {
        void HandleError(ErrorContext errorContext);
    }
}