using Microsoft.Data.SqlClient;
using System.Data;
using MssqlToolBox.Models;
using System;
using DriveInfo = MssqlToolBox.Models.DriveInfo;

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
        public enum ShowTopQueriesSortBy
        {
            CpuTime,
            ElapsedTime
        }
        public static DataTable ShowTopQueries(string databaseName, ShowTopQueriesSortBy sortBy)
        {
            var query = @"SELECT TOP 10 * FROM (SELECT query_stats.query_hash AS Query_Hash,   
            SUM(query_stats.total_worker_time) / SUM(query_stats.execution_count) AS Avg_CPU_Time, 
	        SUM(query_stats.total_elapsed_time) / SUM(query_stats.execution_count) AS Avg_Elapsed_Time, 
            MIN(query_stats.statement_text) AS Sample_Statement_Text,
            MAX(last_execution_time) AS Last_Execution_Time,
            MAX(last_elapsed_time) AS Last_Elapsed_Time,
            SUM(execution_count) AS Total_Execution_Count,
			db_name(CONVERT(SMALLINT, MIN(query_stats.database_id))) db_name
             FROM   
                (
                    SELECT 
					dep.value database_id,
                        QS.*,   
                        SUBSTRING(ST.text, (QS.statement_start_offset/2) + 1,  
                        ((CASE statement_end_offset   
                            WHEN -1 THEN DATALENGTH(ST.text)  
                            ELSE QS.statement_end_offset END   
                                - QS.statement_start_offset)/2) + 1) AS statement_text  
                     FROM sys.dm_exec_query_stats AS QS  
                     CROSS APPLY sys.dm_exec_sql_text(QS.sql_handle) as ST
					 CROSS APPLY sys.dm_exec_plan_attributes(QS.plan_handle) dep
					 WHERE dep.attribute = N'dbid'					
                ) as query_stats  
             GROUP BY 
            query_stats.query_hash) as t1 WHERE db_name=@dbName";

            switch (sortBy)
            {
                case ShowTopQueriesSortBy.CpuTime:
                    query += " ORDER BY 2 DESC;";
                    break;
                case ShowTopQueriesSortBy.ElapsedTime:
                    query += " ORDER BY 3 DESC;";
                    break;
            }
            var parameters = new[]
            {
                new SqlParameter("@dbName", SqlDbType.NVarChar) { Value = databaseName }
            };
            return GetDataTable(query, parameters);
        }
        public static DataTable ShowTopActiveQueries(string databaseName)
        {
            var query = @"SELECT TOP 10 * FROM (SELECT db_name(req.database_id) db_name,
            c.client_net_address,
            req.status,
            req.command,
            s.original_login_name,
            s.login_time,
            s.program_name,
            s.client_interface_name,
            req.session_id,
            req.start_time,
            req.cpu_time AS cpu_time_ms,            
            REPLACE(
                REPLACE(
                    SUBSTRING(
                        ST.text, (req.statement_start_offset / 2) + 1,
                        ((CASE req.statement_end_offset
                                WHEN -1 THEN DATALENGTH(ST.text)
                                ELSE req.statement_end_offset
                            END - req.statement_start_offset
                            ) / 2
                        ) + 1
                    ), CHAR(10), ' '
                ), CHAR(13), ' '
            ) AS statement_text
            FROM
            sys.dm_exec_requests AS req
            inner join sys.dm_exec_connections AS c on req.connection_id=c.connection_id  
            inner JOIN sys.dm_exec_sessions AS s  
            ON c.session_id = s.session_id 
            CROSS APPLY sys.dm_exec_sql_text(req.sql_handle) AS ST
            ) as t1 WHERE db_name=@dbName
            ORDER BY cpu_time_ms DESC";

            var parameters = new[]
            {
                new SqlParameter("@dbName", SqlDbType.NVarChar) { Value = databaseName }
            };
            return GetDataTable(query, parameters);
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
        public static ServerStatusModel GetServerStatus()
        {
            ServerStatusModel serverStatusModel = new();
            var query = @"
        SELECT 
            total_physical_memory_kb / 1024 AS Ram_Size_MB,
            (total_physical_memory_kb - available_physical_memory_kb) / 1024 AS Used_Ram_Size_MB,
            available_physical_memory_kb / 1024 AS Free_Ram_Size_MB,
            100 - (CAST(available_physical_memory_kb AS FLOAT) / CAST(total_physical_memory_kb AS FLOAT)) * 100 AS Ram_Usage_Percentage,
            sqlserver_start_time AS Sql_Server_Start_Time
        FROM 
            sys.dm_os_sys_memory
        CROSS JOIN 
            sys.dm_os_sys_info;
"
                ;
            var memoryInfo = GetDataTable(query);

            var row = memoryInfo.Rows[0];
            serverStatusModel.RamSizeMB = (long)row["Ram_Size_MB"];
            serverStatusModel.UsedRamSizeMB = (long)row["Used_Ram_Size_MB"];
            serverStatusModel.FreeRamSizeMB = (long)row["Free_Ram_Size_MB"];
            serverStatusModel.RamUsagePercentage = Math.Round((double)row["Ram_Usage_Percentage"], 2);
            serverStatusModel.SqlServerStartTime = (DateTime)row["Sql_Server_Start_Time"];

            var driveInfos = GetDataTable("EXEC xp_fixeddrives;");
            serverStatusModel.DriveInfos = new();
            foreach (DataRow driveInfo in driveInfos.Rows)
            {
                serverStatusModel.DriveInfos.Add(
                    new DriveInfo
                    {
                        DriveLetter = (string)driveInfo["drive"],
                        FreeSpaceMB = (int)driveInfo["MB free"]
                    });
            }

            return serverStatusModel;
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
