using System.ComponentModel.DataAnnotations;
using NAxonFramework.Serialization;

namespace NAxonFramework.EventHandling.Saga.Repository
{
    public abstract class AbstractSagaEntry<T>
    {
        protected AbstractSagaEntry(object saga, string sagaIdentifier, ISerializer serializer)
        {
            SagaId = sagaIdentifier;
            var serialized = serializer.Serialize(saga, typeof(T));
            SerializedSaga = (T)serialized.Data;
            SagaType = serialized.Type.Name;
            Revision = serialized.Type.Revision;
        }

        protected AbstractSagaEntry()
        {
        }

        [Key]
        public string SagaId { get; set; }
        public string SagaType { get; set; }
        public string Revision { get; set; }
        
        public T SerializedSaga { get; set; }
    }
}