using System;

namespace NAxonFramework.CommandHandling.Model.Inspection
{
    public interface IAggregateMetaModelFactory
    {
        IAggregateModel CreateModel(Type aggregateType);
    }
}