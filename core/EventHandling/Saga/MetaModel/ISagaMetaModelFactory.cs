namespace NAxonFramework.EventHandling.Saga.MetaModel
{
    public interface ISagaMetaModelFactory
    {
        ISagaModel ModelOf<T>();
    }
}