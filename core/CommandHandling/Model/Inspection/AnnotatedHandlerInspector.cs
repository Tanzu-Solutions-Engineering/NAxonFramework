using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public class AnnotatedHandlerInspector
    {
        private readonly Type _inspectedType;
        private IParameterResolverFactory _parameterResolverFactory;
        private Dictionary<Type, AnnotatedHandlerInspector> _registry;
        //todo: rename to baseclass
        private List<AnnotatedHandlerInspector> _superClassInspectors;
        private List<IMessageHandlingMember> _handlers;
        private IHandlerDefinition _handlerDefinition;
        
        private AnnotatedHandlerInspector(Type inspectedType,
            List<AnnotatedHandlerInspector> superClassInspectors,
        IParameterResolverFactory parameterResolverFactory,
            IHandlerDefinition handlerDefinition,
            Dictionary<Type, AnnotatedHandlerInspector> registry)
        {
            _inspectedType = inspectedType;
            _superClassInspectors = superClassInspectors;
            _parameterResolverFactory = parameterResolverFactory;
            _handlerDefinition = handlerDefinition;
            _registry = registry;
        }
        public static AnnotatedHandlerInspector InspectType<T>() 
        {
            return InspectType<T>(ClasspathParameterResolverFactory.Factory);
        }
        public static AnnotatedHandlerInspector InspectType(Type type) 
        {
            return InspectType(type, ClasspathParameterResolverFactory.Factory);
        }
        public static AnnotatedHandlerInspector InspectType<T>(IParameterResolverFactory parameterResolverFactory) 
        {
            return InspectType<T>(parameterResolverFactory, ClasspathHandlerDefinition.Factory);
        }

        public static AnnotatedHandlerInspector InspectType(Type type, IParameterResolverFactory parameterResolverFactory)
        {
            return InspectType(type, parameterResolverFactory, ClasspathHandlerDefinition.Factory);
        }
        public static AnnotatedHandlerInspector InspectType<T>(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            return InspectType(typeof(T), parameterResolverFactory, handlerDefinition);
        }

        public static AnnotatedHandlerInspector InspectType(Type type, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            return CreateInspector(type,
                parameterResolverFactory,
                handlerDefinition,
                new Dictionary<Type, AnnotatedHandlerInspector>());
        }
        

        private static AnnotatedHandlerInspector CreateInspector(Type inspectedType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, Dictionary<Type, AnnotatedHandlerInspector> registry)
        {

            if (!registry.ContainsKey(inspectedType))
            {
                registry.Add(inspectedType,
                    AnnotatedHandlerInspector.Initialize(inspectedType,
                        parameterResolverFactory,
                        handlerDefinition,
                        registry));
            }
            return registry[inspectedType];
        }

        private static AnnotatedHandlerInspector Initialize(Type inspectedType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, Dictionary<Type, AnnotatedHandlerInspector> registry)
        {
            var parents = new List<AnnotatedHandlerInspector>();
                    foreach(var iFace in inspectedType.GetInterfaces()) {
                        //noinspection unchecked
                        parents.Add(CreateInspector(iFace,
                                                    parameterResolverFactory,
                                                    handlerDefinition,
                                                    registry));
                    }
                    if (inspectedType.BaseType != null && inspectedType.BaseType != typeof(object)) {
                        parents.Add(CreateInspector(inspectedType.BaseType,
                                                    parameterResolverFactory,
                                                    handlerDefinition,
                                                    registry));
                    }
                    AnnotatedHandlerInspector inspector = new AnnotatedHandlerInspector(inspectedType,
                                                                                             parents,
                                                                                             parameterResolverFactory,
                                                                                             handlerDefinition,
                                                                                             registry);
                    inspector.InitializeMessageHandlers(parameterResolverFactory, handlerDefinition);
                    return inspector;
        }

        //todo: can be optimized over java as .net reflection allows querying parent members as well
        private void InitializeMessageHandlers(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            foreach (var method in _inspectedType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                handlerDefinition.CreateHandler(_inspectedType, method, parameterResolverFactory)
                    .IfPresent(RegisterHandler);
            }

            foreach (var constructor in _inspectedType.GetConstructors())
            {
                handlerDefinition.CreateHandler(_inspectedType, constructor, parameterResolverFactory)
                    .IfPresent(RegisterHandler);
            }

            _superClassInspectors.ForEach(sci => _handlers.AddRange(Handlers));
            _handlers.Sort(HandlerComparator.Instance);
        }
        private void RegisterHandler(IMessageHandlingMember handler) {
            _handlers.Add(handler);
        }

        public List<IMessageHandlingMember> Handlers => _handlers;
    }


}