using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using MoreLinq;
using NAxonFramework.CommandHandling.Model;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling
{
    public class AggregateAnnotationCommandHandler<T> : IMessageHandler<ICommandMessage>
    {
        private readonly IRepository<T> _repository;
        private readonly ICommandTargetResolver _commandTargetResolver;
        private readonly IAggregateModel _aggregateModel;
        private readonly IReadOnlyDictionary<string, IMessageHandler<ICommandMessage>> _handlers;

        public AggregateAnnotationCommandHandler(IRepository<T> repository) : this(repository, new AnnotationCommandTargetResolver())
        {
            
        }

        public AggregateAnnotationCommandHandler(IRepository<T> repository, ICommandTargetResolver commandTargetResolver) 
            : this(repository, commandTargetResolver, ClasspathParameterResolverFactory.Factory)
        {
            
        }

        public AggregateAnnotationCommandHandler(IRepository<T> repository,
            ICommandTargetResolver commandTargetResolver,
            IParameterResolverFactory parameterResolverFactory) 
            : this(repository, commandTargetResolver, AnnotatedAggregateMetaModelFactory.InspectAggregate(typeof(T),parameterResolverFactory))
        {
            
        }

        public AggregateAnnotationCommandHandler(IRepository<T> repository,
            ICommandTargetResolver commandTargetResolver,
            IParameterResolverFactory parameterResolverFactory,
            IHandlerDefinition handlerDefinition)
            :this(repository, commandTargetResolver, AnnotatedAggregateMetaModelFactory.InspectAggregate(typeof(T), parameterResolverFactory, handlerDefinition))
        {
        }

        public AggregateAnnotationCommandHandler(IRepository<T> repository,
            ICommandTargetResolver commandTargetResolver,
            IAggregateModel aggregateModel)
        {
            if(aggregateModel == null) throw new ArgumentNullException(nameof(aggregateModel));
            if(repository == null) throw new ArgumentNullException(nameof(repository));
            if(commandTargetResolver == null) throw new ArgumentNullException(nameof(commandTargetResolver));
            _repository = repository;
            _commandTargetResolver = commandTargetResolver;
            _aggregateModel = aggregateModel;
        }

        public IDisposable Subscribe(ICommandBus commandBus)
        {
            var subscriptions = new List<IDisposable>();
            foreach (var supportedCommand in SupportedCommandNames)
            {
                var subscription = commandBus.Subscribe(supportedCommand, this);
                if (subscription != null)
                {
                    subscriptions.Add(subscription);
                }
            }
            return new CompositeDisposable(subscriptions);
        }

        private IReadOnlyDictionary<string, IMessageHandler<ICommandMessage>> InitializeHandlers(IAggregateModel aggregateModel)
        {
            var handlersFound = new Dictionary<string, IMessageHandler<ICommandMessage>>();
            var  aggregateCommandHandler = new AggregateCommandHandler(this);
            aggregateModel.CommandHandlers.ForEach(kv =>
            {
                if (kv.Value.Unwrap<ICommandMessageHandlingMember>()
                    .Map(x => x.IsFactoryHandler).OrElse(false))
                {
                    handlersFound[kv.Key] = new AggregateConstructorCommandHandler(kv.Value, this);
                }
                else
                {
                    handlersFound[kv.Key] = aggregateCommandHandler;
                }
            });
            return handlersFound;
        }


        public object Handle(ICommandMessage commandMessage)
        {
            return _handlers.GetValueOrDefault(commandMessage.CommandName).Handle(commandMessage);
        }

        protected object ResolveReturnValue(ICommandMessage command, IAggregate<T> createdAggregate)
        {
            return createdAggregate.Identifier;
        }

        public IEnumerable<string> SupportedCommandNames => _handlers.Keys;


        private class AggregateConstructorCommandHandler : IMessageHandler<ICommandMessage>
        {
            private readonly AggregateAnnotationCommandHandler<T> _parent;
            private readonly IMessageHandlingMember _handler;

            public AggregateConstructorCommandHandler(IMessageHandlingMember handler, AggregateAnnotationCommandHandler<T> parent)
            {
                _parent = parent;
                _handler = handler;
            }

            public object Handle(ICommandMessage command)
            {
                var aggregate = _parent._repository.NewInstance(() => (T) _handler.Handle(command, null));
                return _parent.ResolveReturnValue(command, aggregate);
            }
        }

        private class AggregateCommandHandler : IMessageHandler<ICommandMessage> 
        {
            private readonly AggregateAnnotationCommandHandler<T> _parent;

            public AggregateCommandHandler(AggregateAnnotationCommandHandler<T> parent)
            {
                _parent = parent;
            }

            public Object Handle(ICommandMessage command)
            {
                var iv = _parent._commandTargetResolver.ResolveTarget(command);
                return _parent._repository.Load(iv.Identifier, iv.Version).Handle((ICommandMessage<T>)command);
            }
        }
    }
} 