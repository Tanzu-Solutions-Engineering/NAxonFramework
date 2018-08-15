namespace NAxonFramework.EventHandling.Saga
{
    public class SagaInitializationPolicy
    {
        public static SagaInitializationPolicy None = new SagaInitializationPolicy(SagaCreationPolicy.None, null);
        public SagaCreationPolicy CreationPolicy { get; }
        public AssociationValue InitialAssociationValue { get; }

        public SagaInitializationPolicy(SagaCreationPolicy creationPolicy, AssociationValue initialAssociationValue)
        {
            CreationPolicy = creationPolicy;
            InitialAssociationValue = initialAssociationValue;
        }
    }
}