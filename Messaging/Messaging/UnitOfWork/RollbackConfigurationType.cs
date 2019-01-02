namespace NAxonFramework.Messaging.UnitOfWork
{
    public enum RollbackConfigurationType
    {
        Never,
        AnyThrowable,
        UncheckedException,
        RuntimeExceptions
    }
}