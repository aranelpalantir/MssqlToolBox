using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListIndexFragmentations
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.GetValidInput("Database Name (* for all databases): ", "Database Name cannot be empty. Please enter a valid Database name.");
            var limit = int.Parse(ConsoleHelpers.GetValidInput("Fragmentation Limit (0 for all indexes): ", "Fragmentation Limit cannot be empty. Please enter a valid Fragmentation limit."));
            var indexFragmentations = DatabaseOperations.GetIndexFragmentations(Program.ConnectionString, databaseName, limit);
            if (indexFragmentations.Count == 0)
            {
                ConsoleHelpers.WriteLineColoredMessage($"There are no index fragmentations exceeding the specified limit of {limit}.", ConsoleColor.DarkYellow);
            }
            else
            {
                var count = 1;
                foreach (var indexName in indexFragmentations.Keys)
                {
                    var indexFragmentation = indexFragmentations[indexName];
                    if (indexFragmentation > 0)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}. Index Name: {indexName}, Fragmentation: {indexFragmentation}", ConsoleColor.Red);
                    }
                    else
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}. Index Name: {indexName}, Fragmentation: {indexFragmentation}", ConsoleColor.Green);
                    }

                    count++;
                }
            }
        }
    }
}
