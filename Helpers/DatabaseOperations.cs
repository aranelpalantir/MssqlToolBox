using Microsoft.Data.SqlClient;
using System.Data;
using MssqlToolBox.Models;

namespace MssqlToolBox.Helpers
{
    internal static class DatabaseOperations
    {
        public static bool TestDatabaseConnection()
        {
            using var connection = new SqlConnection(Program.ConnectionString);
            connection.Open();
            return true;
        }

        public static List<string> GetOnlineDatabases()
        {
            return GetDatabases(true).Select(r => r.Name).ToList();
        }
        public static List<string> GetOfflineDatabases()
        {
            return GetDatabases(false).Select(r => r.Name).ToList();
        }
        public static List<string> GetOnlineDatabasesWithRecoveryModels()
        {
            return GetDatabases(true).Select(r => $"{r.Name} => {r.RecoveryModel}").ToList();
        }
        public static List<string> GetTables(string databaseName)
        {
            var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
            var result = GetDataTable(query, dbName: databaseName);

            return result.AsEnumerable().Select(row => row.Field<string>("TABLE_NAME")).ToList();
        }
        public static Dictionary<string, string> GetRecoveryModels()
        {
            return GetDatabases(true).ToDictionary(db => db.Name, db => db.RecoveryModel);
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
        public static List<IndexModel> GetIndexFragmentations(string databaseName, string tableName, int limit = 0)
        {
            var indexFragmantations = new List<IndexModel>();

            var databases = (databaseName == "*") ? GetOnlineDatabases() : [databaseName];

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

                var result = GetDataTable(query, parameters: [.. parametersList], dbName: dbName);

                indexFragmantations.AddRange(from DataRow row in result.Rows
                                             select new IndexModel
                                             {
                                                 DatabaseName = dbName,
                                                 TableName = row["TableName"].ToString(),
                                                 Name = row["IndexName"].ToString(),
                                                 Fragmentation = Convert.ToDouble(row["Fragmentation"])
                                             });
            }

            return indexFragmantations;
        }
        public static void ChangeRecoveryModel(string databaseName, RecoveryModel recoveryModel)
        {
            var recoveryModelStr = recoveryModel.ToString();

            using var connection = new SqlConnection(Program.ConnectionString);
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
        public static double? GetIndexFragmentation(string databaseName, string tableName, string indexName)
        {
            var query = IndexQuery + " AND t.name=@tableName AND ix.name=@indexName";
            var parameters = new[]
            {
                new SqlParameter("@tableName", SqlDbType.NVarChar) { Value = tableName },
                new SqlParameter("@indexName", SqlDbType.NVarChar) { Value = indexName }
            };

            var result = GetDataTable(query, parameters, dbName: databaseName);

            return (result.Rows.Count > 0) ? Convert.ToDouble(result.Rows[0]["Fragmentation"]) : null;
        }

        public static void RebuildIndex(string databaseName, string tableName, string indexName)
        {
            ExecuteIndexOperation(databaseName, tableName, indexName, "REBUILD");
        }

        public static void ReorganizeIndex(string databaseName, string tableName, string indexName)
        {
            ExecuteIndexOperation(databaseName, tableName, indexName, "REORGANIZE");
        }

        public static void UpdateStatistics(string databaseName, string tableName, string indexName)
        {
            ExecuteIndexOperation(databaseName, tableName, indexName, "UPDATE STATISTICS");
        }

        private static void ExecuteIndexOperation(string databaseName, string tableName, string indexName, string operation)
        {
            using var connection = new SqlConnection(Program.ConnectionString);
            connection.Open();
            connection.ChangeDatabase(databaseName);

            var sql = $"ALTER INDEX [{indexName}] ON [{tableName}] {operation};";

            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static List<DatabaseModel> GetDatabases(bool online)
        {
            var query = "SELECT name, recovery_model_desc FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb') ";
            query += online ? " AND state = 0" : " AND state <> 0";
            query += " ORDER BY Name; ";

            var result = GetDataTable(query);

            return (from DataRow row in result.Rows
                    select new DatabaseModel
                    {
                        Name = row["name"].ToString(),
                        RecoveryModel = row["recovery_model_desc"].ToString()
                    }).ToList();
        }

        private static DataTable GetDataTable(string query,
            SqlParameter[]? parameters = null, string? dbName = null)
        {
            var dataTable = new DataTable();

            using var connection = new SqlConnection(Program.ConnectionString);
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
