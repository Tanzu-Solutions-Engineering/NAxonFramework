using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NAxonFramework.CommandHandling.Model.Inspection;
using NAxonFramework.Common;
using NAxonFramework.Messaging.Attributes;
using NAxonFramework.Messaging.UnitOfWork;
using static NAxonFramework.Common.Assert;

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

        protected AbstractRepository(IParameterResolverFactory parameterResolverFactory, IHandlerDefinition handlerDefinition)
        :this(AnnotatedAggregateMetaModelFactory.InspectAggregate(typeof(T), 
            NotNull(parameterResolverFactory, () => "parameterResolverFactory may not be null"), 
            NotNull(handlerDefinition, () => "handler definition may not be null")))
        {
            
        }
        
        protected AbstractRepository(IAggregateModel aggregateModel) {
            NotNull(aggregateModel, () => "aggregateModel may not be null");
            AggregateModel = aggregateModel;
        }

        public IAggregate<T> NewInstance(Func<T> factoryMethod)
        {
            A aggregate = DoCreateNew(factoryMethod);
            Assert.IsTrue(AggregateModel.EntityClass.IsAssignableFrom(aggregate.RootType), () => "Unsuitable aggregate for this repository: wrong type");
            var uow = CurrentUnitOfWork.Get();
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
            CurrentUnitOfWork.Get().OnPrepareCommit(u =>
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
            });
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

    }
}