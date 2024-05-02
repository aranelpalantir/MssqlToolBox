using MssqlToolBox.Helpers;
using System;
using System.Drawing;

namespace MssqlToolBox.Operations
{
    internal static class ListIndexDetails
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
                var results = DatabaseOperations.GetIndexDetails(dbName, tableName);
                if (results is { Count: > 0 })
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database: {dbName} Index Details:", ConsoleColor.Blue);
                    foreach (var indexDetail in results)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}", ConsoleColor.DarkYellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Table Name: {indexDetail.TableName}, Index Name: {indexDetail.Name}, Index Type: {indexDetail.IndexType}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Columns: {indexDetail.ColumnNames}", ConsoleColor.Green);
                        ConsoleHelpers.WriteLineColoredMessage($"Included Columns: {indexDetail.IncludedColumnNames}", ConsoleColor.Green);
                        ConsoleHelpers.WriteLineColoredMessage($"Seeks: {indexDetail.Seeks} | Scans: {indexDetail.Scans} | Lookups: {indexDetail.Lookups} | Updates: {indexDetail.Updates}", ConsoleColor.Green);
                        var fragmentationColor = indexDetail.Fragmentation switch
                        {
                            > 0 and <= 30 => ConsoleColor.DarkYellow,
                            > 30 => ConsoleColor.Red,
                            _ => ConsoleColor.Green
                        };
                        ConsoleHelpers.WriteLineColoredMessage($"Fragmentation: {indexDetail.Fragmentation}", fragmentationColor);
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
