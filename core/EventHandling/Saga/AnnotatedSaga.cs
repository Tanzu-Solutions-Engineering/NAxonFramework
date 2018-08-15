using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAxonFramework.Common;
using NAxonFramework.EventHandling.Saga.MetaModel;

namespace NAxonFramework.EventHandling.Saga
{
    public class AnnotatedSaga<T> : SagaLifecycle, ISaga<T>
    {
        private readonly T _sagaInstance;
        private readonly ISagaModel _metaModel;
        private ITrackingToken _trackingToken;
        private bool _isActive;

        public AnnotatedSaga(string sagaId, ISet<AssociationValue> associationValues, T annotatedSaga, ITrackingToken trackingToken, ISagaModel metaModel)
        {
            _sagaInstance = annotatedSaga;
            _metaModel = metaModel;
            SagaIdentifier = sagaId;
            AssociationValues = new AssociationValuesImpl(associationValues);
            _trackingToken = trackingToken;
        }

        public string SagaIdentifier { get; }

        public IAssociationValues AssociationValues { get; }

        public R Invoke<R>(Func<T, R> invocation)
        {
            try 
            {
                return ExecuteWithResult(() => invocation.Invoke(_sagaInstance));
            } catch (Exception e) 
            {
                throw new SagaExecutionException("Exception while invoking a Saga", e);
            }
        }

        public void Execute(Action<T> invocation)
        {
            base.Execute(() => invocation(_sagaInstance));
        }

        public bool CanHandle(IEventMessage @event)
        {
            return IsActive && _metaModel.FindHandlerMethods(@event).Any(h => AssociationValues.Contains(h.GetAssociationValue(@event)));
        }

        public void Handle(IEventMessage @event)
        {
            if (IsActive)
            {
                _metaModel
                    .FindHandlerMethods(@event)
                    .FirstOrDefault(h => AssociationValues.Contains(h.GetAssociationValue(@event)))
                    .IfPresent(h =>
                    {
                        try
                        {
                            ExecuteWithResult(() => h.Handle(@event, _sagaInstance));
                            if (@event is ITrackedEventMessage)
                            {
                                Interlocked.Exchange(ref _trackingToken, ((ITrackedEventMessage) @event).TrackingToken);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new SagaExecutionException("Exception while handling an Event in a Saga", e);
                        }
                        finally
                        {
                            if (h.IsEndingHandler)
                            {
                                DoEnd();
                            }
                        }
                        
                    });
            }
        }

        public bool IsActive { get; }

        public ITrackingToken TrackingToken => _trackingToken;
        public T Root => _sagaInstance;

        protected override void DoEnd() => _isActive = false;

        protected override void DoAssociateWith(AssociationValue property) => AssociationValues.Add(property);
        protected override void DoRemoveAssociation(AssociationValue property) => AssociationValues.Remove(property);


        void ISaga<T>.PrepareReset()
        {
            throw new NotImplementedException();
        }

        void IEventListener.PrepareReset() => throw new ResetNotSupportedException("Sagas do not support reset");

        public bool SupportsReset => false;
    }
}