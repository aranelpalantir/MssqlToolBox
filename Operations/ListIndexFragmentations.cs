using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListIndexFragmentations
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.GetValidInput("Database Name (* for all databases): ", "Database Name cannot be empty. Please enter a valid Database name.");
            var tableName = "*";
            if (databaseName != "*")
            {
                tableName = ConsoleHelpers.GetValidInput("Table Name (* for all tables): ",
                    "Table Name cannot be empty. Please enter a valid Table name.");
            }
            var limit = int.Parse(ConsoleHelpers.GetValidInput("Fragmentation Limit (0 for all indexes): ", "Fragmentation Limit cannot be empty. Please enter a valid Fragmentation limit."));
            var indexFragmentations = DatabaseOperations.GetIndexFragmentations(Program.ConnectionString, databaseName, tableName, limit);
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
