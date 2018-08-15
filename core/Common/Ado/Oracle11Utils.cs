using System.Data;
using Dapper;

namespace NAxonFramework.Common.Ado
{
    public static class Oracle11Utils
    {
        public static void SimulateAutoIncrement(IDbConnection connection, string tableName, string columnName)
        {
            string sequenceName = tableName + "_seq";
            string triggerName = tableName + "_id";

            connection.Execute("CREATE sequence " + sequenceName + " start with 1 increment by 1 nocycle");
            connection.Execute(
                $"create or replace trigger {triggerName} " +
                $"before insert on {tableName} " +
                "for each row " +
                "begin " +
                $"  :new.{columnName} := {sequenceName}.nextval; " +
                "end;"
            );
        }
    }
}