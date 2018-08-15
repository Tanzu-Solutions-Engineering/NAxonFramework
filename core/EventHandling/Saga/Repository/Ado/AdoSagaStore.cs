using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Extensions.Logging;
using NAxonFramework.Serialization;

namespace NAxonFramework.EventHandling.Saga.Repository.Ado
{
    public class AdoSagaStore : ISagaStore
    {
        private readonly Func<IDbConnection> _connectionProvider;
        private readonly ISagaSqlSchema _sqldef;
        private readonly ISerializer _serializer;
        private readonly ILogger _logger = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILogger<AdoSagaStore>>();

        public AdoSagaStore(Func<IDbConnection> connectionProvider, ISagaSqlSchema sqldef, ISerializer serializer)
        {
            _connectionProvider = connectionProvider;
            _sqldef = sqldef;
            _serializer = serializer;
        }

        public IEntry LoadSaga(Type sagaType, string sagaIdentifier)
        {
            try
            {
                using (var conn = _connectionProvider())
                {
                    conn.Open();
                    var serializedSaga = _sqldef.SqlLoadSaga(conn, sagaIdentifier);
                    if (serializedSaga == null)
                    {
                        return null;
                    }

                    var loadedSaga = _serializer.Deserialize(serializedSaga);
                    _logger.LogDebug($"Loaded saga id [{sagaIdentifier}] of type [{loadedSaga.GetType().Name}]");
                    var associations = _sqldef.SqlFindAssociations(conn, sagaIdentifier, SagaTypeName(sagaType));
                    return new EntryImpl(associations, loadedSaga);
                }
            }
            catch (DataException e)
            {
                throw new SagaStorageException("Exception while loading a Saga", e);
            }
        }
        
        
        public ISet<string> FindSagas(Type sagaType, AssociationValue associationValue)
        {
            try
            {
                using (var conn = _connectionProvider())
                {
                    return _sqldef.SqlFindAssocSagaIdentifiers(conn, associationValue.PropertyKey, associationValue.PropertyValue, SagaTypeName(sagaType));
                }
            }
            catch (DataException e)
            {
                throw new SagaStorageException("Exception while loading a Saga", e);
            }

        }

        public void DeleteSaga(Type sagaType, string sagaIdentifier, ISet<AssociationValue> associationValues)
        {
            try
            {
                using (var conn = _connectionProvider())
                {
                    conn.Open();
                    _sqldef.SqlDeleteAssociationEntries(conn, sagaIdentifier);
                    _sqldef.SqlDeleteSagaEntry(conn, sagaIdentifier);
                }
            }
            catch (DataException e)
            {
                throw new SagaStorageException("Exception while loading a Saga", e);
            }
        }

        public void UpdateSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, IAssociationValues associationValues)
        {
            var entry = new SagaEntry(saga, sagaIdentifier, _serializer);
            _logger.LogDebug($"Updating saga id {sagaIdentifier} as {Encoding.UTF8.GetString(entry.SerializedSaga)}");
            int updateCount;
            try
            {
                using (var conn = _connectionProvider())
                {
                    conn.Open();
                    updateCount = _sqldef.SqlUpdateSaga(conn, entry.SagaId, entry.SerializedSaga, entry.SagaType, entry.Revision);
                    
                    if (updateCount != 0)
                    {
                        foreach (var associationValue in associationValues.AddedAssociations)
                        {
                            _sqldef.SqlStoreAssocValue(conn, associationValue.PropertyKey, associationValue.PropertyValue, SagaTypeName(sagaType), sagaIdentifier);
                        }
                        foreach (var associationValue in associationValues.RemovedAssociations)
                        {
                            _sqldef.SqlRemoveAssocValue(conn, associationValue.PropertyKey, associationValue.PropertyValue, SagaTypeName(sagaType), sagaIdentifier);
                        }
                    }
                }
                
            }
            catch (DataException e)
            {
                throw new SagaStorageException("Exception while loading a Saga", e);
            }
            
            if (updateCount == 0)
            {
                _logger.LogWarning("Expected to be able to update a Saga instance, but no rows were found. Inserting instead.");
                InsertSaga(sagaType, sagaIdentifier, saga, token, associationValues.AsSet());
            }
        }

        public void InsertSaga(Type sagaType, string sagaIdentifier, object saga, ITrackingToken token, ISet<AssociationValue> associationValues)
        {
            var entry = new SagaEntry(saga, sagaIdentifier, _serializer);
            _logger.LogDebug($"Storing saga id {sagaIdentifier} as {Encoding.UTF8.GetString(entry.SerializedSaga)}");
            
            try
            {
                using (var conn = _connectionProvider())
                {
                    conn.Open();
                    _sqldef.SqlStoreSaga(conn, entry.SagaId, entry.Revision, entry.SagaType, entry.SerializedSaga);

                    foreach (var associationValue in associationValues)
                    {
                        _sqldef.SqlStoreAssocValue(conn, associationValue.PropertyKey, associationValue.PropertyValue, SagaTypeName(sagaType), sagaIdentifier);
                    }
                }
            }
            catch (DataException e)
            {
                throw new SagaStorageException("Exception while loading a Saga", e);
            }
        }

        private string SagaTypeName(Type sagaType)
        {
            return _serializer.TypeForClass(sagaType).Name;
        }

        public void CreateSchema()
        {
            using (var connection = _connectionProvider())
            {
                connection.Open();
                _sqldef.SqlCreateTableSagaEntry(connection);
                _sqldef.SqlCreateTableAssocValueEntry(connection);
            }
            
        }

        private class EntryImpl : IEntry
        {
            public EntryImpl(ISet<AssociationValue> associationValues, object saga)
            {
                AssociationValues = associationValues;
                Saga = saga;
            }

            public ITrackingToken TrackingToken => null;
            public ISet<AssociationValue> AssociationValues { get; }
            public object Saga { get; }
        }
    }

    // todo: refactor towards async
    public interface ISagaSqlSchema
    {
        ISerializedObject SqlLoadSaga(IDbConnection connection, string sagaId);
        int SqlRemoveAssocValue(IDbConnection connection, string key, string value, string sagaType, string sagaIdentifier);
        int SqlStoreAssocValue(IDbConnection connection, string key, string value, string sagaType, string sagaIdentifier);
        ISet<string> SqlFindAssocSagaIdentifiers(IDbConnection connection, string key, string value, string sagaType);
        ISet<AssociationValue> SqlFindAssociations(IDbConnection connection, string sagaIdentifier, string sagaType);
        int SqlDeleteSagaEntry(IDbConnection connection, string sagaIdentifier);
        int SqlDeleteAssociationEntries(IDbConnection connection, string sagaIdentifier);
        int SqlUpdateSaga(IDbConnection connection, string sagaIdentifier, byte[] serializedSaga, string sagaType, string revision);
        int SqlStoreSaga(IDbConnection connection, string sagaIdentifier, string revision, string sagaType, byte[] serializedSaga);
        void SqlCreateTableAssocValueEntry(IDbConnection connection);
        void SqlCreateTableSagaEntry(IDbConnection connection);
        string ReadToken(IDataReader resultSet);

    }
}