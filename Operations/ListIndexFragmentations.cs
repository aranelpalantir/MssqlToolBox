using MssqlToolBox.Helpers;
using System;
using static MssqlToolBox.Helpers.DatabaseOperations;

namespace MssqlToolBox.Operations
{
    internal static class ListIndexFragmentations
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.SelectDatabase();
            if (databaseName == null)
            {
                ConsoleHelpers.WriteLineColoredMessage("Database selection cancelled or invalid. Operation aborted.", ConsoleColor.DarkYellow);
                return;
            }

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
            var limit = int.Parse(ConsoleHelpers.GetValidInput("Fragmentation Limit (0 for all indexes): ", "Fragmentation Limit cannot be empty. Please enter a valid Fragmentation limit."));
            
            var databases = new List<string>();
            if (databaseName == "*")
                databases = DatabaseOperations.GetOnlineDatabases();
            else
                databases.Add(databaseName);

            foreach (var dbName in databases)
            {
                var indexFragmentations = DatabaseOperations.GetIndexFragmentations(dbName, tableName, limit);
                if (indexFragmentations.Count == 0)
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database Name: {dbName} =>There are no index fragmentations exceeding the specified limit of {limit}.", ConsoleColor.DarkYellow);
                }
                else
                {
                    var count = 1;
                    foreach (var index in indexFragmentations)
                    {
                        var color = index.Fragmentation switch
                        {
                            > 0 and <= 30 => ConsoleColor.DarkYellow,
                            > 30 => ConsoleColor.Red,
                            _ => ConsoleColor.Green
                        };
                        ConsoleHelpers.WriteLineColoredMessage($"{count}. Database Name: {index.DatabaseName} => Index Name: {index.Name}, Fragmentation: {index.Fragmentation}", color);
                        count++;
                    }
                }
                ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);
            }
        }
    }
}
