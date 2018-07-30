namespace NAxonFramework.Messaging.UnitOfWork
{
    public enum Phase
    {
        NOT_STARTED,
        STARTED, 
        PREPARE_COMMIT, 
        COMMIT, 
        ROLLBACK, 
        AFTER_COMMIT, 
        CLEANUP, 
        CLOSED 
        
    }
}