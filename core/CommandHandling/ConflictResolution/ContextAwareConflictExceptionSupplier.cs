namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public delegate T ContextAwareConflictExceptionSupplier<out T>(IConflictDescription conflictDescription);
}