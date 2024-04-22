using MssqlToolBox.Helpers;
using System.Data;

namespace MssqlToolBox.Operations
{
    internal static class ShowTopQueries
    {
        public static void Execute(DatabaseOperations.ShowTopQueriesSortBy sortBy)
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
                var results = DatabaseOperations.ShowTopQueries(dbName, sortBy);
                if (results is { Rows.Count: > 0 })
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database: {dbName} Top 50 Queries by {sortBy}:", ConsoleColor.Blue);
                    foreach (DataRow row in results.Rows)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}", ConsoleColor.DarkYellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Avg. CPU Time: {row["Avg_CPU_Time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Avg. Elapsed Time: {row["Avg_Elapsed_Time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Total Execution Count: {row["Total_Execution_Count"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Last Execution Time: {row["Last_Execution_Time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Last Elapsed Time: {row["Last_Elapsed_Time"]}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Sample Statement Text: {row["Sample_Statement_Text"]}", ConsoleColor.Yellow);
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
