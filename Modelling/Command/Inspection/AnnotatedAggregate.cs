using System;
using System.Collections.Generic;
using System.Linq;
using NAxonFramework.Common;
using NAxonFramework.EventHandling;
using NAxonFramework.EventSourcing;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AnnotatedAggregate<T> : AggregateLifecycle, IAggregate<T>, IApplyMore
    {
        private IAggregateModel _inspector;
        private IRepositoryProvider _repositoryProvider;
        private Queue<Action> _delayedTasks = new Queue<Action>();
        private IEventBus _eventBus;
        private T _aggregateRoot;
        private bool _applying = false;
        private bool _isDeleted = false;
        private long _lastKnownSequence;

        protected AnnotatedAggregate(T aggregateRoot, IAggregateModel model, IEventBus eventBus) 
            : this(aggregateRoot, model, eventBus, null)
        {
            
        }
        protected AnnotatedAggregate(T aggregateRoot, IAggregateModel model, IEventBus eventBus, IRepositoryProvider repositoryProvider) 
            : this(model, eventBus, repositoryProvider)
        {
            this._aggregateRoot = aggregateRoot;
        }

        protected AnnotatedAggregate(IAggregateModel inspector, IEventBus eventBus)
            : this(inspector, eventBus, (IRepositoryProvider)null)
        {
            
        }

        protected AnnotatedAggregate(IAggregateModel inspector, IEventBus eventBus, IRepositoryProvider repositoryProvider)
        {
            _inspector = inspector;
            _repositoryProvider = repositoryProvider;
            _eventBus = eventBus;
        }
        
        public static AnnotatedAggregate<T> Initialize(Func<T> aggregateFactory, IAggregateModel aggregateModel, IEventBus eventBus)
        {
            return Initialize(aggregateFactory, aggregateModel, eventBus, false);
        }

        public static AnnotatedAggregate<T> Initialize(Func<T> aggregateFactory, IAggregateModel aggregateModel,
            IEventBus eventBus, IRepositoryProvider repositoryProvider)
        {
            return Initialize(aggregateFactory, aggregateModel, eventBus, repositoryProvider, false);
        }

        public static AnnotatedAggregate<T> Initialize(Func<T> aggregateFactory, IAggregateModel aggregateModel,
            IEventBus eventBus, bool generateSequences)
        {
            return Initialize(aggregateFactory, aggregateModel, eventBus, null, generateSequences);
        }
        
        public static AnnotatedAggregate<T> Initialize(Func<T> aggregateFactory, IAggregateModel aggregateModel,
            IEventBus eventBus, IRepositoryProvider repositoryProvider,
            bool generateSequences)
        {
            var aggregate = new AnnotatedAggregate<T>(aggregateModel, eventBus, repositoryProvider);
            if (generateSequences) 
            {
                aggregate.InitSequence();
            }
            aggregate.RegisterRoot(aggregateFactory);
            return aggregate;
        }
        public static AnnotatedAggregate<T> Initialize(T aggregateRoot, IAggregateModel aggregateModel,
            IEventBus eventBus)
        {
            return Initialize(aggregateRoot, aggregateModel, eventBus, null);
        }
        public static AnnotatedAggregate<T> Initialize(T aggregateRoot, IAggregateModel aggregateModel,
            IEventBus eventBus, IRepositoryProvider repositoryProvider)
        {
            var aggregate = new AnnotatedAggregate<T>(aggregateRoot,
                aggregateModel,
                eventBus,
                repositoryProvider);
            return aggregate;
        }
        
        public void InitSequence() => InitSequence(-1);

        private void InitSequence(long lastKnownSequenceNumber)
        {
            _lastKnownSequence = lastKnownSequenceNumber;
        }

        protected void RegisterRoot(Func<T> aggregateFactory)
        {
            _aggregateRoot = ExecuteWithResult(aggregateFactory);
            Execute(() =>
            {
                _applying = true;
                while (!_delayedTasks.IsEmpty())
                {
                    _delayedTasks.Dequeue().Invoke();
                }

                _applying = false;
            });
        }

        public string Type => _inspector.Type;
        public object Identifier => _inspector.GetIdentifier(_aggregateRoot);
        public override long? Version => _inspector.GetVersion(_aggregateRoot);
        public long? LastSequence => _lastKnownSequence == -1 ? (long?)null : _lastKnownSequence;
        public override bool IsLive => true;


        protected override IAggregate<R> DoCreateNew<R>(Func<R> factoryMethod)
        {
            if (_repositoryProvider == null)
            {
                throw new AxonConfigurationException($"Since repository provider is not provided, we cannot spawn a new aggregate for {typeof(R)}");
            }

            var repository = _repositoryProvider.RepositoryFor<R>();
            if (repository == null)
            {
                throw new InvalidOperationException($"There is no configured repository for {typeof(R)}");
            }

            return repository.NewInstance(factoryMethod);
        }


        public R Invoke<R>(Func<T, R> invocation)
        {
            try 
            {
                return ExecuteWithResult(() => invocation.Invoke(_aggregateRoot));
            } 
            catch (Exception e) 
            {
                throw new AggregateInvocationException("Exception occurred while invoking an aggregate", e);
            }
        }

        public void Execute(Action<T> invocation)
        {
            Execute(() => invocation.Invoke(_aggregateRoot));
        }

        public bool IsDeleted => _isDeleted;

        public Type RootType => _aggregateRoot.GetType();

        public override void MarkDeleted() => _isDeleted = true;

        protected void Publish(IEventMessage msg)
        {
            if (msg is IDomainEventMessage message)
            {
                _lastKnownSequence = message.SequenceNumber;
            }
            _inspector.Publish(msg, _aggregateRoot);
            PublishOnEventBus(msg);
        }

        private void PublishOnEventBus(IEventMessage msg)
        {
            _eventBus?.Publish(msg);
        }

        public object handle(IMessage message)
        {

            Func<Object> messageHandling;

            if (message is ICommandMessage) {
                messageHandling = () => Handle((ICommandMessage) message);
            } 
            else if (message is IEventMessage) 
            {
                messageHandling = () => Handle((IEventMessage) message);
            } 
            else 
            {
                throw new ArgumentException($"Unsupported message type: {message.GetType()}");
            }

            return ExecuteWithResult(messageHandling);
        }

        public object Handle(ICommandMessage msg)
        {
            return ExecuteWithResult(() =>
            {
                var interceptors = _inspector.CommandHandlerInterceptors
                    .Where(chi => chi.CanHandle(msg))
                    .OrderBy(x => x.Priority)
                    .Select(x => new AnnotatedCommandHandlerInterceptor(x, _aggregateRoot))
                    .ToList();
                var handler = _inspector.CommandHandlers.Values
                    .Where(x => x.CanHandle(msg))
                    .FirstOrDefault();
                if (handler == null)
                {
                    throw new NoHandlerForCommandException($"No handler available to handle command {msg.CommandName}");
                }
                object result;
                if (interceptors.IsEmpty())
                {
                    result = handler.Handle(msg, _aggregateRoot);
                }
                else
                {
                    result = new DefaultInterceptorChain(CurrentUnitOfWork.Get(), interceptors.GetEnumerator(), 
                        MessageHandler<ICommandMessage>.Create(m => handler.Handle(msg, _aggregateRoot)))
                        .Proceed();
                }

                if (_aggregateRoot == null)
                {
                    _aggregateRoot = (T) result;
                    return this.IdentifierAsString();
                }

                return result;
            });
        }
        private Object Handle(IEventMessage eventMessage) 
        {
            _inspector.Publish(eventMessage, _aggregateRoot);
            return null;
        }

        protected override IApplyMore DoApply<P>(P payload, MetaData metaData)
        {
            if (!_applying && _aggregateRoot != null)
            {
                _applying = true;
                try
                {
                    Publish(CreateMessage(payload, metaData));
                    while (!_delayedTasks.IsEmpty())
                    {
                        _delayedTasks.Dequeue().Invoke();
                    }
                }
                finally
                {
                    _delayedTasks.Clear();
                    _applying = false;
                }
            }
            else
            {
                _delayedTasks.Enqueue(() => Publish(CreateMessage(payload,metaData)));
            }

            return this;
        }

        private IEventMessage CreateMessage<P>(P payload, MetaData metaData)
        {
            if (_lastKnownSequence != null)
            {
                var seq = _lastKnownSequence + 1;
                var id = this.IdentifierAsString();
                if (id == null)
                {
                    Assert.State(seq == 0, () => "The aggregate identifier has not been set. It must be set at the latest when applying the creation event");
                    return new LazyIdentifierDomainEventMessage<P>(this, Type, seq, payload, metaData);
                }
                return new GenericDomainEventMessage(this.Type, this.IdentifierAsString(), seq, payload, metaData);
            }
            return new GenericEventMessage(payload, metaData);
        }

        public IApplyMore AndThenApply<T>(Func<T> payloadOrMessageSupplier)
        {
            return AndThen<T>(() => ApplyMessageOrPayload(payloadOrMessageSupplier.Invoke()));
        }

        public IApplyMore AndThen<T1>(Action action)
        {
            if (_applying || _aggregateRoot == null)
            {
                _delayedTasks.Enqueue(action);
            }
            else
            {
                action();
            }

            return this;
        }

        private void ApplyMessageOrPayload(object payloadOrMessage)
        {
            if (payloadOrMessage is IMessage message)
            {
                Apply(message.Payload, message.MetaData);
            }
            else if (payloadOrMessage != null)
            {
                Apply(payloadOrMessage, MetaData.EmptyInstance);
            }
        }

        private class LazyIdentifierDomainEventMessage<P> : GenericDomainEventMessage<P>
        {
            private readonly IAggregate _aggregate;

            public LazyIdentifierDomainEventMessage(IAggregate aggregate, String type, long seq, P payload, MetaData metaData) : base(type, null, seq, payload, metaData)
            {
                _aggregate = aggregate;
            }

            public override string AggregateIdentifier => _aggregate.IdentifierAsString();
            public override IMessage WithMetaData(IReadOnlyDictionary<string, object> newMetaData)
            {
                var identifier = _aggregate.IdentifierAsString();
                if (identifier != null)
                {
                    return new GenericDomainEventMessage(Type, AggregateIdentifier, SequenceNumber, Payload, MetaData, Identifier, Timestamp);
                } 
                else 
                {
                    return new LazyIdentifierDomainEventMessage<P>(_aggregate, Type, SequenceNumber, Payload, MetaData.From(newMetaData));
                }
            }

            public override IMessage AndMetaData(IReadOnlyDictionary<string, object> additionalMetaData)
            {
                var identifier = _aggregate.IdentifierAsString();
                if (identifier != null)
                {
                    return new GenericDomainEventMessage(Type, AggregateIdentifier, SequenceNumber, Payload, MetaData, Identifier, Timestamp)
                        .AndMetaData(additionalMetaData);
                } 
                else 
                {
                    return new LazyIdentifierDomainEventMessage<P>(_aggregate, Type, SequenceNumber, Payload, MetaData.MergedWith(additionalMetaData));
                }
            }
        }
    }
}