using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.CommandHandling.Model;
using NAxonFramework.EventSourcing;
using NAxonFramework.EventSourcing.EventStore;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public class DefaultConflictResolver : IConflictResolver
    {
        private IEventStore _eventStore;
        private string _aggregateIdentifier;
        private long _expectedVersion;
        private long _actualVersion;
        private bool _conflictsResolved;
        private List<IDomainEventMessage> _events;

        public DefaultConflictResolver(IEventStore eventStore, string aggregateIdentifier, long expectedVersion, long actualVersion) 
        {
            _eventStore = eventStore;
            _aggregateIdentifier = aggregateIdentifier;
            _expectedVersion = expectedVersion;
            _actualVersion = actualVersion;
        }

        public void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ContextAwareConflictExceptionSupplier<T> exceptionSupplier) where T : Exception
        {
            _conflictsResolved = true;
            List<IDomainEventMessage> unexpectedEvents = UnexpectedEvents();
            if (predicate.Invoke(unexpectedEvents)) 
            {
                T exception = exceptionSupplier.Invoke(new DefaultConflictDescription(_aggregateIdentifier, _expectedVersion, _actualVersion, unexpectedEvents));
                if (exception != null) 
                {
                    throw exception;
                }
            }
        }

        public void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ConflictExceptionSupplier<T> exceptionSupplier) where T : Exception
        {
            DetectConflicts(predicate, cd => exceptionSupplier.Invoke(cd.AggregateIdentifier, cd.ExpectedVersion, cd.ActualVersion));
        }

        public void DetectConflicts(Predicate<List<IDomainEventMessage>> predicate)
        {
            DetectConflicts(predicate, (identifier, version, actualVersion) => new ConflictingAggregateVersionException(identifier,version,actualVersion));
        }
        
        public void EnsureConflictsResolved() 
        {
            if (!_conflictsResolved) {
                throw new ConflictingAggregateVersionException(_aggregateIdentifier, _expectedVersion, _actualVersion);
            }
        }

        private List<IDomainEventMessage> UnexpectedEvents() 
        {
            if (_events == null) 
            {
                if (_expectedVersion >= _actualVersion) 
                {
                    return new List<IDomainEventMessage>();
                }
                _events = _eventStore.ReadEvents(_aggregateIdentifier, _expectedVersion + 1)
                    .Where(@event => @event.SequenceNumber <= _actualVersion)
                    .ToList();
            }
            return _events;
        }
    }
}