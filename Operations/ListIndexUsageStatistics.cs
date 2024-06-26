﻿using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListIndexUsageStatistics
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

            var tableName = "*";
            if (databaseName != "*")
            {
                tableName = ConsoleHelpers.SelectTable(databaseName);
                if (tableName == null)
                {
                    ConsoleHelpers.WriteLineColoredMessage("Table selection cancelled or invalid. Operation aborted.", ConsoleColor.DarkYellow);
                    return;
                }
            }

            foreach (var dbName in databases)
            {
                var count = 1;
                var results = DatabaseOperations.GetIndexUsageStatistics(dbName, tableName);
                if (results is { Count: > 0 })
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database: {dbName} Index Usage Statistics:", ConsoleColor.Blue);
                    foreach (var indexUsageStatistic in results)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}", ConsoleColor.DarkYellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Table Name: {indexUsageStatistic.TableName}, Index Name: {indexUsageStatistic.Name}, Index Type: {indexUsageStatistic.IndexType}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Seeks: {indexUsageStatistic.Seeks} | Scans: {indexUsageStatistic.Scans} | Lookups: {indexUsageStatistic.Lookups} | Updates: {indexUsageStatistic.Updates}", ConsoleColor.Green);
                        ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);

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
