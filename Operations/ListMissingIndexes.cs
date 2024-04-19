using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListMissingIndexes
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
                var results = DatabaseOperations.GetMissingIndexes(dbName);
                if (results is { Count: > 0 })
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Database: {dbName} Top 10 Missing Indexes by Improvement Measure:", ConsoleColor.Blue);
                    foreach (var missingIndex in results)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}", ConsoleColor.DarkYellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Improvement Measure: {missingIndex.ImprovementMeasure}", ConsoleColor.Yellow);
                        ConsoleHelpers.WriteLineColoredMessage($"Create Index Statement: {missingIndex.CreateIndexStatement}", ConsoleColor.Green);
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
