using System;
using System.Collections.Generic;
using NAxonFramework.EventSourcing;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public interface IConflictResolver
    {
        void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ConflictExceptionSupplier<T> exceptionSupplier) where T : Exception;
        void DetectConflicts(Predicate<List<IDomainEventMessage>> predicate);
        void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ContextAwareConflictExceptionSupplier<T> exceptionSupplier) where T : Exception;
    }
}