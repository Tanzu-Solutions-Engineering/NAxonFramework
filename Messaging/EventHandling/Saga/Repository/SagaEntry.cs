using NAxonFramework.Serialization;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public class SagaEntry : AbstractSagaEntry<byte[]>
    {
        public SagaEntry(object saga, string sagaIdentifier, ISerializer serializer) : base(saga, sagaIdentifier, serializer)
        {
        }

        public SagaEntry()
        {
        }
    }
}