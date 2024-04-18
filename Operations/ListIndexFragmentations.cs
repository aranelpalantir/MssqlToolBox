using MssqlToolBox.Helpers;

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
            var indexFragmentations = DatabaseOperations.GetIndexFragmentations(databaseName, tableName, limit);
            if (indexFragmentations.Count == 0)
            {
                ConsoleHelpers.WriteLineColoredMessage($"There are no index fragmentations exceeding the specified limit of {limit}.", ConsoleColor.DarkYellow);
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
                    ConsoleHelpers.WriteLineColoredMessage($"{count}. Index Name: {index.Name}, Fragmentation: {index.Fragmentation}", color);
                    count++;
                }
            }
        }
    }
}
