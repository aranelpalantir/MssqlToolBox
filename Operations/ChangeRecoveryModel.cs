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

            var targetRecoveryModel = GetRecoveryModel();

            var databases = new List<string>();
            if (databaseName == "*")
                databases = DatabaseOperations.GetOnlineDatabasesWithRecoveryModels();
            else
                databases.Add(databaseName);

            foreach (var dbNameWithRecoveryModel in databases)
            {
                var dbName = GetDbName(dbNameWithRecoveryModel);
                var dbRecoveryModel = GetDbRecoveryModel(dbNameWithRecoveryModel);
                if (targetRecoveryModel.Value == TransformRecoveryModel(dbRecoveryModel))
                    continue;
                if (ConsoleHelpers.ConfirmAction($"Are you sure you want to change the recovery model of the database '{dbName}' from '{dbRecoveryModel}' to '{targetRecoveryModel}'? (Y/N)"))
                {
                    DatabaseOperations.ChangeRecoveryModel(dbName, targetRecoveryModel.Value);
                    ConsoleHelpers.WriteLineColoredMessage($"Recovery model of database '{dbName}' has been changed from '{dbRecoveryModel}' to '{targetRecoveryModel}'.", ConsoleColor.Green);
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage($"Change recovery operation cancelled for '{dbName}'.", ConsoleColor.DarkYellow);
                }
            }
        }

        private static string GetDbName(string dbNameWithRecoveryModel)
        {
            return dbNameWithRecoveryModel.Split("=>")[0].Trim();
        }
        private static string GetDbRecoveryModel(string dbNameWithRecoveryModel)
        {
            return dbNameWithRecoveryModel.Split("=>")[1].Trim();
        }
        private static DatabaseOperations.RecoveryModel TransformRecoveryModel(string dbRecoveryModel)
        {
            switch (dbRecoveryModel)
            {
                case "SIMPLE":
                    return DatabaseOperations.RecoveryModel.Simple;
                case "FULL":
                    return DatabaseOperations.RecoveryModel.Full;
                case "BULK_LOGGED":
                    return DatabaseOperations.RecoveryModel.BULK_LOGGED;
                default:
                    throw new Exception("Invalid Db Recovery Model.");
            }
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
