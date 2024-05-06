using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ShrinkDatabase
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
                var filesDetails = DatabaseOperations.GetDatabaseFilesDetails(dbName);
                foreach (var databaseFileModel in filesDetails)
                {
                    ConsoleHelpers.WriteLineColoredMessage($"{databaseFileModel.Name} - Size: {databaseFileModel.Size:N0} MB", ConsoleColor.DarkYellow);
                }

                if (ConsoleHelpers.ConfirmAction($"Are you sure you want to shrink {dbName}? (Y/N)"))
                {
                    DatabaseOperations.ShrinkDatabase(dbName);
                    ConsoleHelpers.WriteLineColoredMessage($"Shrink operation completed successfully for {dbName}.", ConsoleColor.Green);
                    filesDetails = DatabaseOperations.GetDatabaseFilesDetails(dbName);
                    foreach (var databaseFileModel in filesDetails)
                    {
                        ConsoleHelpers.WriteLineColoredMessage($"{databaseFileModel.Name} - Size: {databaseFileModel.Size:N0} MB", ConsoleColor.Green);
                    }
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Shrink operation cancelled for {dbName}.", ConsoleColor.DarkYellow);
                }
                ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);
            }
        }
    }

}
