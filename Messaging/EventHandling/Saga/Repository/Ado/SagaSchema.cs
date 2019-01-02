namespace NAxonFramework.EventHandling.Saga.Repository.Ado
{
    public class SagaSchema
    {
        private const string DEFAULT_SAGA_ENTRY_TABLE = "SagaEntry";
        private const string DEFAULT_ASSOC_VALUE_ENTRY_TABLE = "AssociationValueEntry";

        public string SagaEntryTable { get; }
        public string AssociationValueEntryTable { get; }

        public SagaSchema(string sagaEntryTable, string associationValueEntryTable)
        {
            SagaEntryTable = sagaEntryTable;
            AssociationValueEntryTable = associationValueEntryTable;
        }

        public SagaSchema() : this(DEFAULT_SAGA_ENTRY_TABLE, DEFAULT_ASSOC_VALUE_ENTRY_TABLE)
        {
        }
    }
}