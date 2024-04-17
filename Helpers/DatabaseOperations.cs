using Microsoft.Data.SqlClient;
using System.Data;

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
        public static List<string> GetOnlineDatabases(string connectionString)
        {
            var query = "SELECT name FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb') AND state = 0 ORDER BY Name;";
            var result = GetDataTable(connectionString, query);

            var databases = new List<string>();
            foreach (DataRow row in result.Rows)
            {
                databases.Add(row["name"].ToString());
            }

            return databases;
        }
        public static List<string> GetOfflineDatabases(string connectionString)
        {
            var query = "SELECT name FROM sys.databases WHERE name NOT IN ('master','model','msdb','tempdb') AND state <> 0 ORDER BY Name;";
            var result = GetDataTable(connectionString, query);

            var databases = new List<string>();
            foreach (DataRow row in result.Rows)
            {
                databases.Add(row["name"].ToString());
            }

            return databases;
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
        public static Dictionary<string, double> GetIndexFragmentations(string connectionString, string databaseName, int limit)
        {
            var indexFragmantations = new Dictionary<string, double>();

            var databases = new List<string>();
            if (databaseName == "*")
                databases = GetOnlineDatabases(connectionString);
            else
                databases.Add(databaseName);

            foreach (var dbName in databases)
            {
                var query = @"
                SELECT  t.name AS TableName, ix.name AS IndexName, ips.avg_fragmentation_in_percent AS Fragmentation,ix.object_id,ix.index_id
                FROM sys.indexes ix
                INNER JOIN sys.tables t ON ix.object_id = t.object_id
                INNER JOIN sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'SAMPLED') ips ON ix.object_id = ips.object_id AND ix.index_id = ips.index_id
                WHERE ix.type_desc IN ('NONCLUSTERED', 'CLUSTERED')
                AND t.is_ms_shipped = 0 
                AND ix.name IS NOT NULL
                AND ips.avg_fragmentation_in_percent>=@limit
                ORDER BY Fragmentation DESC";
                var parameters = new SqlParameter("@limit", SqlDbType.Int) { Value = limit };


                var result = GetDataTable(connectionString, query, parameter: parameters, dbName: dbName);

                foreach (DataRow row in result.Rows)
                {
                    var tableName = row["TableName"].ToString();
                    var indexName = row["IndexName"].ToString();
                    var fragmantation = Convert.ToDouble(row["Fragmentation"]);

                    var key = $"{dbName}=> {tableName}.{indexName}";
                    indexFragmantations.Add(key, fragmantation);
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
        private static DataTable GetDataTable(string connectionString, string query, SqlParameter? parameter = null, string? dbName = null)
        {
            var dataTable = new DataTable();

            using var connection = new SqlConnection(connectionString);
            connection.Open();
            if (dbName != null)
                connection.ChangeDatabase(dbName);

            using var command = new SqlCommand(query, connection);
            if (parameter != null)
            {
                command.Parameters.Add(parameter);
            }

            using var adapter = new SqlDataAdapter(command);
            adapter.Fill(dataTable);

            return dataTable;
        }
    }
}
