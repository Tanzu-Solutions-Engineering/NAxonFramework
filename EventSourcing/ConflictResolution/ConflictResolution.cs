using System;
using System.Collections.Generic;
using System.Reflection;
using NAxonFramework.Common;
using NAxonFramework.EventSourcing;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.CommandHandling.ConflictResolution
{
    public class ConflictResolution : IParameterResolverFactory, IParameterResolver<IConflictResolver>
    {
        private static string CONFLICT_RESOLUTION_KEY = nameof(ConflictResolution);
        public static void Initialize(IConflictResolver conflictResolver)
        {
            Assert.State(CurrentUnitOfWork.IsStarted, () => "An active Unit of Work is required for conflict resolution");
            CurrentUnitOfWork.Get().GetOrComputeResource(CONFLICT_RESOLUTION_KEY, key => conflictResolver);
        }

        public static IConflictResolver GetConflictResolver()
        {
            return CurrentUnitOfWork.Map(uow =>
            {
                var conflictResolver = uow.GetResource<IConflictResolver>(CONFLICT_RESOLUTION_KEY);
                return conflictResolver == null ? NoConflictResolver.Instance : conflictResolver;
            }).OrElse(NoConflictResolver.Instance);
        }
        public IParameterResolver CreateInstance(MethodBase executable, ParameterInfo[] parameters, int parameterIndex)
        {
            if (parameters[parameterIndex].ParameterType == typeof(IConflictResolver))
            {
                return this;
            }

            return null;
        }

        public IConflictResolver ResolveParameterValue(IMessage message)
        {
            return GetConflictResolver();
        }

        public bool Matches(IMessage message)
        {
            return message is ICommandMessage;
        }

        public Type SupportedPayloadType => typeof(object);

        object IParameterResolver.ResolveParameterValue(IMessage message)
        {
            return ResolveParameterValue(message);
        }
    }

    public class NoConflictResolver : IConflictResolver
    {
        public static NoConflictResolver Instance { get; } = new NoConflictResolver();
        public void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ConflictExceptionSupplier<T> exceptionSupplier) where T : Exception
        {
            
        }

        public void DetectConflicts(Predicate<List<IDomainEventMessage>> predicate)
        {
            
        }

        public void DetectConflicts<T>(Predicate<List<IDomainEventMessage>> predicate, ContextAwareConflictExceptionSupplier<T> exceptionSupplier) where T : Exception
        {
            
        }
    }
}