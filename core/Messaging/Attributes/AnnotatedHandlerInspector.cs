using System;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.Attributes
{
    public class AnnotatedHandlerInspector
    {
        private readonly Type _inspectedType;
        private readonly IParameterResolverFactory _parameterResolverFactory;
        private readonly Dictionary<Type, AnnotatedHandlerInspector> _registry;
        private readonly List<AnnotatedHandlerInspector> _superClassInspectors;
        public List<IMessageHandlingMember> Handlers { get; }
        private readonly IHandlerDefinition _handlerDefinition;


        public AnnotatedHandlerInspector(Type inspectedType, List<AnnotatedHandlerInspector> superClassInspectors, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, Dictionary<Type, AnnotatedHandlerInspector> registry)
        {
            _inspectedType = inspectedType;
            _parameterResolverFactory = parameterResolverFactory;
            _registry = registry;
            _superClassInspectors = superClassInspectors;
            Handlers = new List<IMessageHandlingMember>();
            _handlerDefinition = handlerDefinition;
        }

        public static AnnotatedHandlerInspector InspectType(Type handlerType)
        {
            return InspectType(handlerType, ClasspathParameterResolverFactory.Factory);
        }

        public static AnnotatedHandlerInspector InspectType(Type handlerType, IParameterResolverFactory parameterResolverFactory)
        {
            return InspectType(handlerType, parameterResolverFactory, ClasspathHandlerDefinition.Factory);

        }

        public static AnnotatedHandlerInspector InspectType(Type handlerType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            return CreateInspector(handlerType, parameterResolverFactory, handlerDefinition, new Dictionary<Type, AnnotatedHandlerInspector>());
        }

        private static AnnotatedHandlerInspector CreateInspector(Type inspectedType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, Dictionary<Type, AnnotatedHandlerInspector> registry)
        {
            if (!registry.ContainsKey(inspectedType))
            {
                registry[inspectedType] = AnnotatedHandlerInspector.Initialize(inspectedType, parameterResolverFactory, handlerDefinition, registry);
            }

            return registry[inspectedType];
        }

        private static AnnotatedHandlerInspector Initialize(Type inspectedType, IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition, Dictionary<Type, AnnotatedHandlerInspector> registry)
        {
            var parents = new List<AnnotatedHandlerInspector>();
            foreach (var iface in inspectedType.GetInterfaces())
            {
                parents.Add(CreateInspector(iface, parameterResolverFactory, handlerDefinition, registry));
            }

            if (inspectedType.BaseType != null && typeof(object) != inspectedType.BaseType)
            {
                parents.Add(CreateInspector(inspectedType.BaseType, parameterResolverFactory, handlerDefinition, registry));
            }
            var inspector = new AnnotatedHandlerInspector(inspectedType, parents, parameterResolverFactory, handlerDefinition, registry);
            inspector.InitializeMessageHandlers(parameterResolverFactory, handlerDefinition);
            return inspector;
        }

        private void InitializeMessageHandlers(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        {
            foreach (var method in _inspectedType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                _handlerDefinition.CreateHandler(_inspectedType, method, parameterResolverFactory)
                    .IfPresent(RegisterHandler);
            }

            foreach (var constructor in _inspectedType.GetConstructors())
            {
                _handlerDefinition.CreateHandler(_inspectedType, constructor, parameterResolverFactory)
                    .IfPresent(RegisterHandler);
            }
            _superClassInspectors.ForEach(sci => Handlers.AddRange(sci.Handlers));
            Handlers.Sort(HandlerComparator.Instance);
        }

        private void RegisterHandler(IMessageHandlingMember handler)
        {
            Handlers.Add(handler);
        }


        public AnnotatedHandlerInspector Inspect(Type entityType)
        {
            return AnnotatedHandlerInspector.CreateInspector(entityType, _parameterResolverFactory, _handlerDefinition, _registry);
        }
    }
}