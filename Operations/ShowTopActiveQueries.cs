using MssqlToolBox.Helpers;
using System.Data;

namespace MssqlToolBox.Operations
{
    internal static class ShowTopActiveQueries
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.SelectDatabase();
            if (databaseName == null)
            {
                ConsoleHelpers.WriteLineColoredMessage("Database selection cancelled or invalid. Operation aborted.", ConsoleColor.DarkYellow);
                return;
            }

            var databases = new List<string>();
            if (databaseName == "*")
                databases = DatabaseOperations.GetOnlineDatabases();
            else
                databases.Add(databaseName);

            foreach (var dbName in databases)
            {
                var count = 1;
                var results = DatabaseOperations.ShowTopActiveQueries(dbName);
                if (results is { Rows.Count: > 0 })
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database: {dbName} Top 10 Active Queries by CPU Time:", ConsoleColor.Blue);
                    foreach (DataRow row in results.Rows)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}", ConsoleColor.DarkYellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Client Net Address: {row["client_net_address"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Client Login Name: {row["original_login_name"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Client Login Time: {row["login_time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Client Program Name: {row["program_name"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Client Interface Name: {row["client_interface_name"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Session Id: {row["session_id"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Request Start Time: {row["start_time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"CPU Time: {row["cpu_time_ms"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Status: {row["status"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Command: {row["command"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Statement Text: {row["statement_text"]}", ConsoleColor.Yellow);
                        Console.WriteLine();
                        count += 1;
                    }
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage($"No query results found for {dbName}.", ConsoleColor.Yellow);
                }
                ConsoleHelpers.WriteLineColoredMessage("---", ConsoleColor.Yellow);
            }
        }
    }
}
