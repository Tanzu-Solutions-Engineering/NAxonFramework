using System;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public delegate T ConflictExceptionSupplier<out T>(string aggregateIdentifier, long expectedVersion, long actualVersion) where T : Exception;
}