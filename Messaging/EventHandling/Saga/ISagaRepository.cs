using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NAxonFramework.EventHandling.Saga
{
    // todo: refactor for async
    public interface ISagaRepository<T>
    {
        ISet<string> Find(AssociationValue associationValue);
        ISaga<T> Load(string sagaIdentifier);
        ISaga<T> CreateInstance(string sagaIdentifier, Func<T> factoryMethod);
    }
}