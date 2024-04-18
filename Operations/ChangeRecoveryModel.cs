using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ChangeRecoveryModel
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.SelectDatabaseWithRecoveryModel();
            if (databaseName == null)
            {
                ConsoleHelpers.WriteLineColoredMessage("Database selection cancelled or invalid. Operation aborted.", ConsoleColor.DarkYellow);
                return;
            }

            var recoveryModel = GetRecoveryModel();

            var databases = new List<string>();
            if (databaseName == "*")
                databases = DatabaseOperations.GetOnlineDatabases();
            else
                databases.Add(RemoveRecoveryModelDescription(databaseName));

            foreach (var dbName in databases)
            {
                if (ConsoleHelpers.ConfirmAction($"Are you sure you want to change the recovery model of database '{dbName}' to '{recoveryModel}'? (Y/N)"))
                {
                    DatabaseOperations.ChangeRecoveryModel(dbName, recoveryModel.Value);
                    ConsoleHelpers.WriteLineColoredMessage($"Recovery model of database '{dbName}' has been changed to '{recoveryModel}'.", ConsoleColor.Green);
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Change recovery operation cancelled for '{dbName}'.", ConsoleColor.DarkYellow);
                }
            }
        }

        private static string RemoveRecoveryModelDescription(string databaseName)
        {
            return databaseName.Split("=>")[0].Trim();
        }
        private static DatabaseOperations.RecoveryModel? GetRecoveryModel()
        {
            ConsoleHelpers.WriteLineColoredMessage("Recovery Model Options:", ConsoleColor.Blue);
            ConsoleHelpers.WriteLineColoredMessage("1- Simple", ConsoleColor.DarkGreen);
            ConsoleHelpers.WriteLineColoredMessage("2- Full", ConsoleColor.DarkGreen);
            ConsoleHelpers.WriteLineColoredMessage("3- BulkLogged", ConsoleColor.DarkGreen);

            while (true)
            {
                var recoveryModelInput = ConsoleHelpers.GetValidInput("Enter Recovery Model Option (1, 2, or 3): ", "Recovery Model cannot be empty. Please enter a valid Recovery model option (1, 2, or 3).");
                switch (recoveryModelInput)
                {
                    case "1":
                        return DatabaseOperations.RecoveryModel.Simple;
                    case "2":
                        return DatabaseOperations.RecoveryModel.Full;
                    case "3":
                        return DatabaseOperations.RecoveryModel.BULK_LOGGED;
                    default:
                        ConsoleHelpers.WriteLineColoredMessage(
                            "Invalid Recovery Model Option. Please enter a valid option (1, 2, or 3).", ConsoleColor.Red);
                        break;
                }
            }
        }
    }
}
