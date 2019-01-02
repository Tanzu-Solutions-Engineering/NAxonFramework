using System;

namespace NAxonFramework.CommandHandling.Model
{
    public interface IRepositoryProvider
    {
        IRepository<T> RepositoryFor<T>();
    }
}