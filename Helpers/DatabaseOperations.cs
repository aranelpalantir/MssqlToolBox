using Microsoft.Data.SqlClient;
using System.Data;
using MssqlToolBox.Models;

namespace MssqlToolBox.Helpers
{
    internal static class DatabaseOperations
    {
        public static bool TestDatabaseConnection(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            return true;
        }
        private static List<string> GetDatabases(string connectionString, bool online)
        {
            var query = "SELECT name FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb') ";
            query += online ? " AND state = 0" : " AND state <> 0";
            query += " ORDER BY Name; ";
            var result = GetDataTable(connectionString, query);

            var databases = new List<string>();
            foreach (DataRow row in result.Rows)
            {
                databases.Add(row["name"].ToString());
            }

            return databases;
        }
        public static List<string> GetOnlineDatabases(string connectionString)
        {
            return GetDatabases(connectionString, true);
        }
        public static List<string> GetOfflineDatabases(string connectionString)
        {
            return GetDatabases(connectionString, false);
        }
        public static Dictionary<string, string> GetRecoveryModels(string connectionString)
        {
            var recoveryModels = new Dictionary<string, string>();

            var query = "SELECT name, recovery_model_desc FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb') AND state = 0 ORDER BY Name";

            var result = GetDataTable(connectionString, query);

            foreach (DataRow row in result.Rows)
            {
                var dbName = row["name"].ToString();
                var recoveryModel = row["recovery_model_desc"].ToString();
                recoveryModels.Add(dbName, recoveryModel);
            }

            return recoveryModels;
        }
        private const string IndexQuery = @"
                SELECT  t.name AS TableName, ix.name AS IndexName, ips.avg_fragmentation_in_percent AS Fragmentation,ix.object_id,ix.index_id
                FROM sys.indexes ix
                INNER JOIN sys.tables t ON ix.object_id = t.object_id
                INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'SAMPLED') ips ON ix.object_id = ips.object_id AND ix.index_id = ips.index_id
                WHERE ix.type_desc IN ('NONCLUSTERED', 'CLUSTERED')
                AND t.is_ms_shipped = 0 
                AND alloc_unit_type_desc='IN_ROW_DATA'
                AND ix.name IS NOT NULL";
        public static List<IndexModel> GetIndexFragmentations(string connectionString, string databaseName, string tableName, int limit = 0)
        {
            var indexFragmantations = new List<IndexModel>();

            var databases = new List<string>();
            if (databaseName == "*")
                databases = GetOnlineDatabases(connectionString);
            else
                databases.Add(databaseName);

            foreach (var dbName in databases)
            {
                var query = IndexQuery;
                query += " AND ips.avg_fragmentation_in_percent>=@limit";

                if (tableName != "*")
                    query += " AND t.name=@tableName";

                query += " ORDER BY Fragmentation DESC";

                var parametersList = new List<SqlParameter>
                {
                    new("@limit", SqlDbType.Int) { Value = limit }
                };

                if (tableName != "*")
                    parametersList.Add(new("@tableName", SqlDbType.NVarChar) { Value = tableName });

                var result = GetDataTable(connectionString, query, parameters: [.. parametersList], dbName: dbName);

                foreach (DataRow row in result.Rows)
                {
                    indexFragmantations.Add(new IndexModel
                    {
                        DatabaseName = dbName,
                        TableName = row["TableName"].ToString(),
                        Name = row["IndexName"].ToString(),
                        Fragmentation = Convert.ToDouble(row["Fragmentation"])
                    });
                }
            }

            return indexFragmantations;
        }
        public static void ChangeRecoveryModel(string connectionString, string databaseName, RecoveryModel recoveryModel)
        {
            var recoveryModelStr = recoveryModel.ToString();

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var sql = $"ALTER DATABASE [{databaseName}] SET RECOVERY {recoveryModelStr};";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public enum RecoveryModel
        {
            Simple,
            Full,
            BULK_LOGGED
        }
        public static double? GetIndexFragmentation(string connectionString, string databaseName, string tableName, string indexName)
        {
            var query = IndexQuery;
            query += " AND t.name=@tableName AND ix.name=@indexName";
            var parametersList = new List<SqlParameter>
                {
                    new("@tableName", SqlDbType.NVarChar) { Value = tableName },
                    new("@indexName", SqlDbType.NVarChar) { Value = indexName }
                };

            var result = GetDataTable(connectionString, query, parameters: [.. parametersList], dbName: databaseName);

            if (result.Rows.Count > 0)
                return Convert.ToDouble(result.Rows[0]["Fragmentation"]);

            return null;
        }
        public static void RebuildIndex(string connectionString, string databaseName, string tableName, string indexName)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(databaseName);

            var sql = $"ALTER INDEX [{indexName}] ON [{tableName}] REBUILD;";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public static void ReorganizeIndex(string connectionString, string databaseName, string tableName, string indexName)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(databaseName);

            var sql = $"ALTER INDEX [{indexName}] ON [{tableName}] REORGANIZE;";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }
        public static void UpdateStatistics(string connectionString, string databaseName, string tableName, string indexName)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            connection.ChangeDatabase(databaseName);

            var sql = $"UPDATE STATISTICS [{tableName}] [{indexName}];";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static DataTable GetDataTable(string connectionString, string query,
            SqlParameter[]? parameters = null, string? dbName = null)
        {
            var dataTable = new DataTable();

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            if (dbName != null)
                connection.ChangeDatabase(dbName);

            using var command = new SqlCommand(query, connection);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);

            return dataTable;
        }
    }
}
