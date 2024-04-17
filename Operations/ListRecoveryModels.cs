using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ListRecoveryModels
    {
        public static void Execute()
        {
            var recoveryModels = DatabaseOperations.GetRecoveryModels(Program.ConnectionString);
            if (recoveryModels.Count == 0)
            {
                ConsoleHelpers.WriteLineColoredMessage("There are no online databases available.", ConsoleColor.DarkYellow);
            }
            else
            {
                var groupedModels = recoveryModels.GroupBy(kv => kv.Value);

                foreach (var group in groupedModels)
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Recovery Model: {group.Key}", ConsoleColor.Blue);
                    var count = 1;
                    foreach (var dbName in group)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{count}. {dbName.Key}", ConsoleColor.DarkGreen);
                        count++;
                    }
                    ConsoleHelpers.WriteLineColoredMessage($"Total number of databases with {group.Key} recovery model: {group.Count()}", ConsoleColor.Yellow);
                }
            }
        }
    }
}
