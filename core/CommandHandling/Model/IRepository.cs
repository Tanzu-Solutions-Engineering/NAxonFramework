using System;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IRepository<T>
    {
        IAggregate<T> Load(String aggregateIdentifier);
        IAggregate<T> Load(String aggregateIdentifier, long expectedVersion);
        IAggregate<T> NewInstance(Func<T> factoryMethod);


    }
}