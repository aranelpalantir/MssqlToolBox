using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListOnlineDatabases
    {
        public static void Execute()
        {
            var databases = DatabaseOperations.GetOnlineDatabases();
            if (databases.Count == 0)
            {
                ConsoleHelpers.WriteLineColoredMessage("There are no online databases available.", ConsoleColor.DarkYellow);
            }
            else
            {
                for (var i = 0; i < databases.Count; i++)
                {
                    var dbName = databases[i];
                    Console.WriteLine($"{i + 1}. {dbName}");
                }
                ConsoleHelpers.WriteLineColoredMessage($"There are {databases.Count} online databases.", ConsoleColor.Yellow);
            }
        }
    }
}
