using System.Collections.Generic;
using System.Data;
using NAxonFramework.Serialization;
using Dapper;
using MoreLinq.Extensions;

namespace NAxonFramework.EventHandling.Saga.Repository.Ado
{
    public class GenericSagaSqlSchema : ISagaSqlSchema
    {
        protected readonly SagaSchema _sagaSchema;
        public virtual ISerializedObject SqlLoadSaga(IDbConnection connection, string sagaId)
        {
            return connection.QueryFirstOrDefault<SimpleSerializedObject<byte[]>>("SELECT serializedSaga as data, sagaType as type, revision "
                                                                    + "FROM {_sagaSchema.SagaEntryTable} WHERE sagaId = @sagaId", new {sagaId});
        }

        public virtual int SqlRemoveAssocValue(IDbConnection connection, string key, string value, string sagaType, string sagaIdentifier)
        {
            return connection.Execute($"DELETE FROM {_sagaSchema.AssociationValueEntryTable} " 
                                             + "WHERE associationKey = @key AND associationValue = @value"
                                             + " AND sagaType = @sagaType AND sagaId = @sagaIdentifier", new {key, value, sagaType, sagaIdentifier});
        }

        public virtual int SqlStoreAssocValue(IDbConnection connection, string key, string value, string sagaType, string sagaIdentifier)
        {
            return connection.Execute($"INSERT INTO {_sagaSchema.AssociationValueEntryTable} "
                           + " (associationKey, associationValue, sagaType, sagaId) "
                            + " VALUES(@key, @value, @sagaType, @sagaIdentifier)", new {key, value, sagaType, sagaIdentifier});
        }

        public virtual ISet<string> SqlFindAssocSagaIdentifiers(IDbConnection connection, string key, string value, string sagaType)
        {
            
            return new SortedSet<string>(connection.Query<string>($"SELECT sagaId FROM {_sagaSchema.AssociationValueEntryTable}"
                                                     + " WHERE associationKey = @key"
                                                     + " AND associationValue = @value"
                                                     + " AND sagaType = @sagaType", new {key, value, sagaType}));
        }

        public virtual ISet<AssociationValue> SqlFindAssociations(IDbConnection connection, string sagaIdentifier, string sagaType)
        {
            return new HashSet<AssociationValue>(connection.Query<AssociationValue>($"SELECT associationKey, associationValue FROM {_sagaSchema.AssociationValueEntryTable}"
                                                                   + " WHERE sagaId = @sagaIdentifier"
                                                                   + " AND sagaType = @sagaType", new {sagaIdentifier, sagaType}));
        }

        public virtual int SqlDeleteSagaEntry(IDbConnection connection, string sagaIdentifier)
        {
            return connection.Execute($"DELETE FROM {_sagaSchema.SagaEntryTable} WHERE sagaId = @sagaIdentifier", new {sagaIdentifier});
        }

        public virtual int SqlDeleteAssociationEntries(IDbConnection connection, string sagaIdentifier)
        {
            return connection.Execute($"DELETE FROM {_sagaSchema.AssociationValueEntryTable} WHERE sagaId = @sagaIdentifier", new {sagaIdentifier});
        }

        public virtual int SqlUpdateSaga(IDbConnection connection, string sagaIdentifier, byte[] serializedSaga, string sagaType, string revision)
        {
            return connection.Execute($"UPDATE {_sagaSchema.SagaEntryTable} SET serializedSaga = @serializedSaga, revision = @revision WHERE sagaId = @sagaIdentifier",
                new {serializedSaga, sagaType, revision});
        }

        public virtual int SqlStoreSaga(IDbConnection connection, string sagaIdentifier, string revision, string sagaType, byte[] serializedSaga)
        {
            return connection.Execute($"INSERT INTO {_sagaSchema.SagaEntryTable} (sagaId, revision, sagaType, serializedSaga) " 
                                      + "VALUES (@sagaIdentifier,@revision,@sagaType,@serializedSaga)",
                new {sagaIdentifier, revision, sagaType, serializedSaga});
        }

        public virtual void SqlCreateTableAssocValueEntry(IDbConnection connection)
        {
            connection.Execute($"create table {_sagaSchema.AssociationValueEntryTable} (\n" +
                               "        id int not null AUTO_INCREMENT,\n" +
                               "        associationKey varchar(255),\n" +
                               "        associationValue varchar(255),\n" +
                               "        sagaId varchar(255),\n" +
                               "        sagaType varchar(255),\n" +
                               "        primary key (id)\n" +
                               "    );");
        }

        public virtual void SqlCreateTableSagaEntry(IDbConnection connection)
        {
            connection.Execute($"create table {_sagaSchema.SagaEntryTable} (\n" +
                               "        sagaId varchar(255) not null,\n" +
                               "        revision varchar(255),\n" +
                               "        sagaType varchar(255),\n" +
                               "        serializedSaga blob,\n" +
                               "        primary key (sagaId)\n" +
                               "    );");
        }

        

        public virtual string ReadToken(IDataReader resultSet)
        {
            return null;
        }
    }
}