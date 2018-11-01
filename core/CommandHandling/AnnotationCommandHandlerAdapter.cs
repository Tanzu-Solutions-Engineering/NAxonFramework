using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using MoreLinq;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling
{
    public class AnnotationCommandHandlerAdapter : IMessageHandler<ICommandMessage>, ISupportedCommandNamesAware
    {
        private object _target;
        private IAggregateModel _modelInspector;

        public AnnotationCommandHandlerAdapter(object annotatedCommandHandler,
            IParameterResolverFactory parameterResolverFactory,
            IHandlerDefinition handlerDefinition)
        {
            Assert.NotNull(annotatedCommandHandler, () => "annotatedCommandHandler may not be null");
            _modelInspector = AnnotatedAggregateMetaModelFactory.InspectAggregate(annotatedCommandHandler.GetType(), parameterResolverFactory, handlerDefinition);
            _target = annotatedCommandHandler;
        }


        public IDisposable Subscribe(ICommandBus commandBus)
        {
            var subscriptions = SupportedCommandNames
                .Select(supportedCommand => commandBus.Subscribe(supportedCommand, this))
                .ToList();
            return Disposable.Create(() => subscriptions.ForEach(x => x.Dispose()));
        }

        public object Handle(ICommandMessage command)
        {
            return _modelInspector.CommandHandler(command.CommandName).Handle(command, _target);
        }

        public object Handle(IMessage message)
        {
            return Handle((ICommandMessage) message);
        }

        public ISet<string> SupportedCommandNames => _modelInspector.CommandHandlers.Keys.ToHashSet();
    }
}