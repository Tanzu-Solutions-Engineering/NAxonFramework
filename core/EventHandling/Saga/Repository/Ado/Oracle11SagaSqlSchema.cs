using System.Data;
using Dapper;
using NAxonFramework.Common.Ado;

namespace NAxonFramework.EventHandling.Saga.Repository.Ado
{
    public class Oracle11SagaSqlSchema : GenericSagaSqlSchema
    {
        public override void SqlCreateTableAssocValueEntry(IDbConnection connection)
        {
            connection.Execute($"create table {_sagaSchema.AssociationValueEntryTable} (\n" +
                               "        id number(38) not null,\n" +
                               "        associationKey varchar(255),\n" +
                               "        associationValue varchar(255),\n" +
                               "        sagaId varchar(255),\n" +
                               "        sagaType varchar(255),\n" +
                               "        primary key (id)\n" +
                               "    )");
            Oracle11Utils.SimulateAutoIncrement(connection, _sagaSchema.AssociationValueEntryTable, "id");
        }
    }
}