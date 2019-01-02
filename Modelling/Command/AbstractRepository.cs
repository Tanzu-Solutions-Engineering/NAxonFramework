using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Common;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;
using static NAxonFramework.Common.Assert;
using static NAxonFramework.Common.BuilderUtils;

namespace NAxonFramework.CommandHandling.Model
{
    public abstract class AbstractRepository<T, A> : IRepository<T> where A : IAggregate<T>
    {
        private readonly String _aggregatesKey;
        public IAggregateModel AggregateModel { get; }

        protected AbstractRepository(IParameterResolverFactory parameterResolverFactory) 
        : this(AnnotatedAggregateMetaModelFactory.InspectAggregate(typeof(T), NotNull(parameterResolverFactory, () => "parameterResolverFactory may not be null")))
        {
            
        }
//
//        protected AbstractRepository(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
//        :this(AnnotatedAggregateMetaModelFactory.InspectAggregate(typeof(T), 
//            NotNull(parameterResolverFactory, () => "parameterResolverFactory may not be null"), 
//            NotNull(handlerDefinition, () => "handler definition may not be null")))
//        {
//            
//        }
//        
//        protected AbstractRepository(IAggregateModel aggregateModel) {
//            NotNull(aggregateModel, () => "aggregateModel may not be null");
//            AggregateModel = aggregateModel;
//        }

        protected AbstractRepository(Builder<T> builder) 
        {
            builder.validate();
            AggregateModel = builder.buildAggregateModel();
        }
        
        public IAggregate<T> NewInstance(Func<T> factoryMethod)
        {
            var uow = CurrentUnitOfWork.Get();

            A aggregate = DoCreateNew(factoryMethod);
            uow.OnPrepareCommit(x => PrepareForCommit(aggregate));
            Assert.IsTrue(AggregateModel.EntityClass.IsAssignableFrom(aggregate.RootType), () => "Unsuitable aggregate for this repository: wrong type");
            var aggregates = ManagedAggregates(uow);
            IsTrue(aggregates.PutIfAbsent(aggregate.IdentifierAsString(), aggregate) == null, () => "The Unit of Work already has an Aggregate with the same identifier");
            uow.OnRollback(u => aggregates.Remove(aggregate.IdentifierAsString()));
            PrepareForCommit(aggregate);
            return aggregate;
        }

        protected abstract A DoCreateNew(Func<T> factoryMethod);

        public IAggregate<T> Load(string aggregateIdentifier, long? expectedVersion)
        {
            var uow = CurrentUnitOfWork.Get();
            var aggregates = ManagedAggregates(uow);
            A aggregate =
                aggregates.ComputeIfAbsent(aggregateIdentifier, s => DoLoad(aggregateIdentifier, expectedVersion));
            uow.OnRollback(u => aggregates.Remove(aggregateIdentifier));
            ValidateOnLoad(aggregate, expectedVersion);
            PrepareForCommit(aggregate);

            return aggregate;
        }

        protected Dictionary<string, A> ManagedAggregates(IUnitOfWork uow) 
        {
            return uow.Root().GetOrComputeResource(_aggregatesKey, s => new Dictionary<string, A>());
        }

        public IAggregate<T> Load(string aggregateIdentifier)
        {
            return Load(aggregateIdentifier, null);
        }

        private void ValidateOnLoad(IAggregate<T> aggregate, long? expectedVersion)
        {
            if (expectedVersion != null && aggregate.Version != null && !expectedVersion.Equals(aggregate.Version))
            {
                throw new ConflictingAggregateVersionException(aggregate.IdentifierAsString(),
                    expectedVersion.Value,
                    // ReSharper disable once PossibleInvalidOperationException
                    aggregate.Version.Value);
            }
        }

        private void PrepareForCommit(A aggregate)
        {
            if (Phase.STARTED.IsBefore(CurrentUnitOfWork.Get().Phase))
            {
                DoCommit(aggregate);
            }
            else
            {
                CurrentUnitOfWork.Get().OnPrepareCommit(u =>
                {
                    // If the aggregate isn't "managed" anymore, it means its state was invalidated by a rollback
                    DoCommit(aggregate);
                });
            }
        }
        public void DoCommit(A aggregate)
        {

            // if the aggregate isn't "managed" anymore, it means its state was invalidated by a rollback  
            if (ManagedAggregates(CurrentUnitOfWork.Get()).ContainsValue(aggregate))
            {
                if (aggregate.IsDeleted)
                {
                    DoDelete(aggregate);
                }
                else
                {
                    DoSave(aggregate);
                }

                if (aggregate.IsDeleted)
                {
                    PostDelete(aggregate);
                }
                else
                {
                    PostSave(aggregate);
                }
            }
            else
            {
                ReportIllegalState(aggregate);
            }
        
        }

        private void ReportIllegalState(A aggregate)
        {
            throw new AggregateRolledBackException(aggregate.IdentifierAsString());
        }

        protected abstract void DoSave(A aggregate);
        protected abstract A DoLoad(string aggregateIdentifier, long? expoectedVersion);
        protected abstract void DoDelete(A aggregate);

        protected void PostSave(A aggregate)
        {
            // no op by default
        }
        protected void PostDelete(A aggregate)
        {
            // no op by default
        }

        public void Send(IMessage message, IScopeDescriptor scopeDescription)
        {
            if (CanResolve(scopeDescription))
            {
                var aggregateIdentifier = ((AggregateScopeDescriptor) scopeDescription).Identifier.ToString();
                try
                {
                    Load(aggregateIdentifier).Handle(message);
                }
                catch (AggregateNotFoundException e)
                {
                    _logger.Debug(
                        $"Aggregate (with id: [{aggregateIdentifier}]) cannot be loaded. Hence, message '[{message}]' cannot be handled.");
                }
            }
        }
        public bool CanResolve(IScopeDescriptor scopeDescription) 
        {
            return scopeDescription is AggregateScopeDescriptor
                && object.Equals(AggregateModel.Type, ((AggregateScopeDescriptor) scopeDescription).Type);
        }

        public abstract class Builder<T>
        {
            protected Type _aggregateType;
            private IParameterResolverFactory _parameterResolverFactory;
            private IHandlerDefinition _handlerDefinition;
            private IAggregateModel _aggregateModel;
            
            protected Builder(Type aggregateType) 
            {
                _aggregateType = aggregateType;
            }
            
            public Builder<T> ParameterResolverFactory(IParameterResolverFactory parameterResolverFactory) 
            {
                AssertNonNull(parameterResolverFactory, "ParameterResolverFactory may not be null");
                _parameterResolverFactory = parameterResolverFactory;
                return this;
            }
            public Builder<T> HandlerDefinition(IHandlerDefinition handlerDefinition) 
            {
                AssertNonNull(handlerDefinition, "HandlerDefinition may not be null");
                _handlerDefinition = handlerDefinition;
                return this;
            }
            public Builder<T> AggregateModel(IAggregateModel aggregateModel) 
            {
                AssertNonNull(aggregateModel, "AggregateModel may not be null");
                _aggregateModel = aggregateModel;
                return this;
            }
            protected IAggregateModel BuildAggregateModel() 
            {
                if (_aggregateModel == null) 
                {
                    return InspectAggregateModel();
                } 
                else 
                {
                    return _aggregateModel;
                }
            }

            private IAggregateModel InspectAggregateModel() 
            {
                if (_parameterResolverFactory == null && _handlerDefinition == null) 
                {
                    return AnnotatedAggregateMetaModelFactory.InspectAggregate(_aggregateType);
                } 
                else if (_parameterResolverFactory != null && _handlerDefinition == null) 
                {
                    _handlerDefinition = ClasspathHandlerDefinition.Factory..ForClass(aggregateType);
                }
                return AnnotatedAggregateMetaModelFactory.inspectAggregate(
                    aggregateType, parameterResolverFactory, handlerDefinition
                );
            }
        }
    }
}