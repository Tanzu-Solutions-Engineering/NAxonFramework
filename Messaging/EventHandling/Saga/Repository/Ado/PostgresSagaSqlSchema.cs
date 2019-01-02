using System.Data;
using Dapper;

namespace NAxonFramework.EventHandling.Saga.Repository.Ado
{
    public class PostgresSagaSqlSchema : GenericSagaSqlSchema
    {
        public override void SqlCreateTableAssocValueEntry(IDbConnection connection)
        {
            connection.Execute($"create table {_sagaSchema.AssociationValueEntryTable} (\n" +
                               "        id bigserial not null,\n" +
                               "        associationKey varchar(255),\n" +
                               "        associationValue varchar(255),\n" +
                               "        sagaId varchar(255),\n" +
                               "        sagaType varchar(255),\n" +
                               "        primary key (id)\n" +
                               "    );\n");
        }

        public override void SqlCreateTableSagaEntry(IDbConnection connection)
        {
            connection.Execute($"create table {_sagaSchema.SagaEntryTable} (\n" +
                               "        sagaId varchar(255) not null,\n" +
                               "        revision varchar(255),\n" +
                               "        sagaType varchar(255),\n" +
                               "        serializedSaga bytea,\n" +
                               "        primary key (sagaId)\n" +
                               "    );");
        }
    }
}