using System;
using System.Data.SqlClient;
using Storage.Extensions;

namespace Storage.Data
{
    public abstract class DaoBase
    {
        private string ConnectionStringName { get; }
        public static Func<string, string> GetConnectionStringFunc { get; set; }

        protected DaoBase(string connectionStringName) 
            => ConnectionStringName = connectionStringName;

        private bool DoesTableExist(SqlConnection connection, string tableName)
        {
            var sql = "SELECT count(*) as IsExists FROM dbo.sysobjects where id = object_id('[dbo].[" + tableName + "]')";

            return true;
        }

        protected void EnsureTableExists(SqlConnection connection, string tableName)
        {
            if (!DoesTableExist(connection, tableName))
                CreateTable(connection);
        }

        protected SqlConnection GetConnection()
        {
            var connectionString = GetConnectionStringFunc(ConnectionStringName);
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        protected abstract void CreateTable(SqlConnection connection);
    }
};

