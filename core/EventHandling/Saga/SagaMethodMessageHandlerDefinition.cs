using System;
using System.Collections.Generic;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.EventHandling.Saga
{
    public class SagaMethodMessageHandlerDefinition : IHandlerEnhancerDefinition
    {
        private Dictionary<Type, IAssociationResolver> _associationResolverMap;

        public SagaMethodMessageHandlerDefinition(Dictionary<Type, IAssociationResolver> associationResolverMap)
        {
            _associationResolverMap = associationResolverMap;
        }

        public IMessageHandlingMember WrapHandler(Type type, IMessageHandlingMember original)
        {
            var annotationAttributes = original.AnnotationAttributes(typeof(SagaEventHandlerAttribute));
            var creationPolicy = original.AnnotationAttributes(typeof(StartSagaAttribute))
                .Map(attr => ((bool)attr.GetValueOrDefault("forceNew", false)) ? SagaCreationPolicy.Always : SagaCreationPolicy.IfNoneFound).OrElse(SagaCreationPolicy.None);

            return annotationAttributes
                .Map(attr => DoWrapHandler(original, creationPolicy, (string) attr.GetValueOrDefault("keyName"),
                    (string)attr.GetValueOrDefault("associationProperty"),
                    (Type)attr.GetValueOrDefault("associationResolver")))
                .OrElse(original);
        }

        private IMessageHandlingMember DoWrapHandler(IMessageHandlingMember original, SagaCreationPolicy creationPolicy, string associationKeyName, string associationPropertyName, Type associationResolverClass)
        {
            var associationKey = AssociationKey(associationKeyName, associationPropertyName);
            var associationResolver = FindAssociationResolver(associationResolverClass);
            associationResolver.Validate(associationPropertyName, original);
            var endingHandler = original.HasAttribute(typeof(EndSagaAttribute));
            return new SagaMethodMessageHandlingMember(original, creationPolicy, associationKey, associationPropertyName, associationResolver, endingHandler);
        }
        
        private String AssociationKey(String keyName, String associationProperty) 
        {
            return string.Empty.Equals(keyName) ? associationProperty : keyName;
        }

        private IAssociationResolver FindAssociationResolver(Type associationResolverClass) 
        {
            return this._associationResolverMap.ComputeIfAbsent(associationResolverClass, InstantiateAssociationResolver);
        }
        private IAssociationResolver InstantiateAssociationResolver(Type associationResolverClass) 
        {
            try 
            {
                return (IAssociationResolver)Activator.CreateInstance(associationResolverClass);
            }
            catch(Exception e)
            {
                throw new AxonConfigurationException($"`AssociationResolver` {associationResolverClass.Name} must define a no-args constructor.");
            }
        }
    }
}