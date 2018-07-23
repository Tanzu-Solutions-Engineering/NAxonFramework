using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using MoreLinq.Extensions;
using NAxonFramework.Common;
using NAxonFramework.Common.Attributes;
using NAxonFramework.EventHandling;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AnnotatedAggregateMetaModelFactory : IAggregateMetaModelFactory
    {
        private Dictionary<Type, AnnotatedAggregateModel> _registry = new Dictionary<Type, AnnotatedAggregateModel>();
        private IParameterResolverFactory _parameterResolverFactory;
        private IHandlerDefinition _handlerDefinition;


        public AnnotatedAggregateMetaModelFactory() : this(ClasspathParameterResolverFactory.Factory)
        {
        }

        public AnnotatedAggregateMetaModelFactory(IParameterResolverFactory parameterResolverFactory) : this(parameterResolverFactory, ClasspathHandlerDefinition.Factory)
        {
        }
        public AnnotatedAggregateMetaModelFactory(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            _parameterResolverFactory = parameterResolverFactory;
            _handlerDefinition = handlerDefinition;
        }
        
        public static IAggregateModel InspectAggregate(Type aggregateType) {
            return new AnnotatedAggregateMetaModelFactory().CreateModel(aggregateType);
        }
        public static IAggregateModel InspectAggregate(Type aggregateType, IParameterResolverFactory parameterResolverFactory) {
            return new AnnotatedAggregateMetaModelFactory(parameterResolverFactory).CreateModel(aggregateType);
        }
        public static IAggregateModel InspectAggregate(Type aggregateType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition) {
            return new AnnotatedAggregateMetaModelFactory(parameterResolverFactory, handlerDefinition).CreateModel(aggregateType);
        }

        IAggregateModel IAggregateMetaModelFactory.CreateModel(Type aggregateType) => CreateModel(aggregateType);
        
        public AnnotatedAggregateModel CreateModel(Type aggregateType)
        {
            if (!_registry.ContainsKey(aggregateType))
            {
                var inspector = AnnotatedHandlerInspector.InspectType(aggregateType,
                    _parameterResolverFactory,
                    _handlerDefinition);
                var model = new AnnotatedAggregateModel(aggregateType, inspector, this);
                model.Initialize();
                _registry[aggregateType] = model;
            }
            
            return _registry[aggregateType];
        }
        
        
        public class AnnotatedAggregateModel : IAggregateModel
        {
            private List<IChildEntity> _children;
            private AnnotatedHandlerInspector _handlerInspector;
            private readonly AnnotatedAggregateMetaModelFactory _parent;
            private List<IMessageHandlingMember> _commandHandlerInterceptors;
            private ConcurrentDictionary<String, IMessageHandlingMember> _commandHandlers;
            private List<IMessageHandlingMember> _eventHandlers;

            private String _aggregateType;
            private PropertyInfo _identifierField;
            private PropertyInfo _versionField;
            private String _routingKey;
            private Type _inspectedType;

            // todo: refactor to remove dependency on service locator
            private IEnumerable<IChildEntityDefinition> ChildEntityDefinitions => CommonServiceLocator.ServiceLocator.Current.GetAllInstances<IChildEntityDefinition>();
            
            public AnnotatedAggregateModel(Type aggregateType, AnnotatedHandlerInspector handlerInspector, AnnotatedAggregateMetaModelFactory parent) 
            {
                this._inspectedType = aggregateType;
                this._commandHandlerInterceptors = new List<IMessageHandlingMember>();
                this._commandHandlers = new ConcurrentDictionary<string, IMessageHandlingMember>();
                this._eventHandlers = new List<IMessageHandlingMember>();
                this._children = new List<IChildEntity>();
                this._handlerInspector = handlerInspector;
                _parent = parent;
            }

            public void Initialize() 
            {
                InspectAggregateType();
                InspectFields();
                PrepareHandlers();
            }

            private void PrepareHandlers() 
            {
                foreach (var handler in _handlerInspector.Handlers) 
                {
                    
                    var commandHandler = handler.Unwrap<ICommandMessageHandlingMember>();
                    var unwrappedCommandHandlerInterceptor = handler.Unwrap<ICommandHandlerInterceptorHandlingMember>();
                    if (commandHandler.IsPresent) 
                    {
                        _commandHandlers.TryAdd(commandHandler.Get().CommandName, handler);
                    } 
                    else if (unwrappedCommandHandlerInterceptor != null) 
                    {
                        _commandHandlerInterceptors.Add(handler);
                    } 
                    else 
                    {
                        _eventHandlers.Add(handler);
                    }
                }
            }

            private void InspectAggregateType()
            {
                _aggregateType = AnnotationUtils.FindAnnotationAttributes(_inspectedType, typeof(AggregateRootAttribute))
                    .Map(map => (string)map.GetValueOrDefault("type"))
                    .Filter(i => i.Length > 0)
                    .OrElse(_inspectedType.Name);
            }

            private void InspectFields()
            {
                foreach (var field in _inspectedType.GetProperties())
                {
                    ChildEntityDefinitions.ForEach(def => def.CreateChildDefinition(field, this).IfPresent(child =>
                    {
                        _children.Add(child);
                        child.CommandHandlers.ForEach(x => _commandHandlers.TryAdd(x.Key, x.Value));
                    }));
                    AnnotationUtils.FindAnnotationAttributes<EntityIdAttribute>(field).IfPresent(attributes =>
                    {
                        _identifierField = field;
                        if (!string.Empty.Equals(attributes.GetValueOrDefault("routingKey")))
                        {
                            _routingKey = (string)attributes.GetValueOrDefault("routingKey");
                        }
                        else
                        {
                            _routingKey = field.Name;
                        }
                    });
                    if (_identifierField == null)
                    {
                        AnnotationUtils.FindAnnotationAttributes<KeyAttribute>(field).IfPresent(a =>
                        {
                            _identifierField = field;
                            _routingKey = field.Name;
                        });
                    }

                    if (_identifierField != null)
                    {
                        if (!_identifierField.PropertyType.IsValidIdentifier())
                        {
                            throw new AxonConfigurationException($"Aggregate identifier type {_identifierField.PropertyType.Name} should override ToString()");
                        }
                    }
                    AnnotationUtils.FindAnnotationAttributes<AggregateVersionAttribute>(field).IfPresent(attributes => _versionField = field);
                }
                
            }
            private AnnotatedAggregateModel RuntimeModelOf(object target) => (AnnotatedAggregateModel)ModelOf(target.GetType());


            public IReadOnlyDictionary<string, IMessageHandlingMember> CommandHandlers => _commandHandlers;

            public IMessageHandlingMember CommandHandler(string commandName)
            {
                if (!_commandHandlers.TryGetValue(commandName, out var handler))
                {
                    throw new NoHandlerForCommandException($"No handler available to handle command {commandName}");
                }

                return handler;
            }

            public IEntityModel ModelOf(Type childEntityType)
            {
                return _parent.CreateModel(childEntityType);
            }

            public Type EntityClass => _inspectedType;

            public void Publish(IEventMessage message, object target)
            {
                if (target != null)
                {
                    RuntimeModelOf(target).DoPublish(message, target);
                }
            }

            private void DoPublish(IEventMessage message, object target)
            {
                GetHandler(message).IfPresent(h =>
                {
                    try
                    {
                        h.Handle(message, target);
                    }
                    catch (Exception e)
                    {
                        throw new MessageHandlerInvocationException($"Error handling event of type {message.PayloadType} in aggregate", e);
                    }
                });
                _children.ForEach(i => i.Publish(message,target));
            }

            public string Type => _aggregateType;

            public long? GetVersion(object target)
            {
                if(!_inspectedType.IsInstanceOfType(target)) throw new ArgumentException($"Parameter {nameof(target)} must be assignable to {_inspectedType}", nameof(target));
                if (_versionField != null)
                    return (long) _versionField.GetValue(target);
                return null;
            }

            public List<IMessageHandlingMember> CommandHandlerInterceptors => _commandHandlerInterceptors;

            protected Optional<IMessageHandlingMember> GetHandler(IMessage message)
            {
                foreach (var handler in _eventHandlers)
                {
                    if(handler.CanHandle(message))
                        return Optional<IMessageHandlingMember>.Of(handler);
                }
                return Optional<IMessageHandlingMember>.Empty;
            }

            public object GetIdentifier(object target)
            {
                if(!_inspectedType.IsInstanceOfType(target)) throw new ArgumentException($"Parameter {nameof(target)} must be assignable to {_inspectedType}", nameof(target));
                
                return _identifierField?.GetValue(target);
            }

            public string RoutingKey => _routingKey;
        }

    }




    public interface ICommandHandlerInterceptorHandlingMember
    {
    }

    public interface ICommandMessageHandlingMember
    {
        string CommandName { get; }
        string RoutingKey { get; }
        bool IsFactoryHandler { get; }
    }
}