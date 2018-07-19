using System;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public abstract class CurrentUnitOfWork
    {
        public static MetaData CorrelationData => throw new NotImplementedException();
    }
}