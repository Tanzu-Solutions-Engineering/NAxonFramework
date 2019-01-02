using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using MoreLinq;
using NAxonFramework.Common;

namespace NAxonFramework.EventHandling.Saga
{
    public abstract class AbstractSagaManager<T> : IEventHandlerInvoker
    {
        private readonly ISagaRepository<T> _sagaRepository;
        protected readonly Func<T> _sagaFactory;
        private readonly IListenerInvocationErrorHandler _listenerInvocationErrorHandler;

        public AbstractSagaManager(ISagaRepository<T> sagaRepository, Func<T> sagaFactory, IListenerInvocationErrorHandler listenerInvocationErrorHandler)
        {
            _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaFactory));
            _sagaFactory = sagaFactory;
            _listenerInvocationErrorHandler = listenerInvocationErrorHandler;
        }

        public void Handle(IEventMessage @event, Segment segment)
        {
            var associationValues = ExtractAssociationValues(@event);
            var sagas =
                // refactor for async in future
                associationValues.SelectMany(associationValue => _sagaRepository.Find(associationValue))
                    .Where(sagaId => MatchesSegment(segment, sagaId))
                    .Select(x => _sagaRepository.Load(x))
                    .Where(saga => saga != null && saga.IsActive)
                    .ToHashSet();
                    
            var sagaOfTypeInvoked = false;
            foreach (var saga in sagas) 
            {
                if (DoInvokeSaga(@event, saga)) 
                {
                    sagaOfTypeInvoked = true;
                }
            }
            var initializationPolicy = GetSagaCreationPolicy(@event);
            if (ShouldCreateSaga(segment, sagaOfTypeInvoked, initializationPolicy)) 
            {
                StartNewSaga(@event, initializationPolicy.InitialAssociationValue, segment);
            }
        }
        private bool ShouldCreateSaga(Segment segment, bool sagaInvoked, SagaInitializationPolicy initializationPolicy) 
        {
            return ((initializationPolicy.CreationPolicy == SagaCreationPolicy.Always
                     || (!sagaInvoked && initializationPolicy.CreationPolicy == SagaCreationPolicy.IfNoneFound)))
                   && segment.Matches(initializationPolicy.InitialAssociationValue);
        }

        private void StartNewSaga(IEventMessage @event, AssociationValue associationValue, Segment segment)
        {
            var newSaga = _sagaRepository.CreateInstance(CreateSagaIdentifier(segment), _sagaFactory);
            newSaga.AssociationValues.Add(associationValue);
            DoInvokeSaga(@event, newSaga);
        }

        protected string CreateSagaIdentifier(Segment segment) 
        {
            String identifier;

            do 
            {
                identifier = IdentifierFactory.Instance.GenerateIdentifier();
            } 
            while (!MatchesSegment(segment, identifier));
            return identifier;
        }

        protected virtual bool MatchesSegment(Segment segment, String sagaId) => segment.Matches(sagaId);

        protected abstract SagaInitializationPolicy GetSagaCreationPolicy(IEventMessage @event);

        protected abstract HashSet<AssociationValue> ExtractAssociationValues(IEventMessage @event);

        private bool DoInvokeSaga(IEventMessage @event, ISaga<T> saga)
        {
            if (saga.CanHandle(@event))
            {
                try
                {
                    saga.Handle(@event);
                }
                catch (Exception e)
                {
                    _listenerInvocationErrorHandler.OnError(e, @event, saga);
                }

                return true;
            }

            return false;
        }

        public abstract bool CanHandle(IEventMessage eventMessage, Segment segment);

        public bool SupportsReset => false;

        public void PerformReset() => throw new ResetNotSupportedException("Sagas do no support resetting tokens");
    }
}