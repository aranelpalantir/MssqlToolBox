using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListOfflineDatabases
    {
        public static void Execute()
        {
            var databases = DatabaseOperations.GetOfflineDatabases();
            if (databases.Count == 0)
            {
                ConsoleHelpers.WriteLineColoredMessage("There are no offline databases available.", ConsoleColor.DarkYellow);
            }
            else
            {
                for (var i = 0; i < databases.Count; i++)
                {
                    var dbName = databases[i];
                    Console.WriteLine($"{i + 1}. {dbName}");
                }
                ConsoleHelpers.WriteLineColoredMessage($"There are {databases.Count} offline databases.", ConsoleColor.Yellow);
            }
        }
    }
}
