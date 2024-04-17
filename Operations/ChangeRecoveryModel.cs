using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ChangeRecoveryModel
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.GetValidInput("Database Name: ", "Database Name cannot be empty. Please enter a valid Database name.");

            ConsoleHelpers.WriteLineColoredMessage("Recovery Model Options:", ConsoleColor.Blue);
            ConsoleHelpers.WriteLineColoredMessage("1- Simple", ConsoleColor.DarkGreen);
            ConsoleHelpers.WriteLineColoredMessage("2- Full", ConsoleColor.DarkGreen);
            ConsoleHelpers.WriteLineColoredMessage("3- BulkLogged", ConsoleColor.DarkGreen);


            DatabaseOperations.RecoveryModel? recoveryModel = null;
            while (recoveryModel == null)
            {
                var recoveryModelInput = ConsoleHelpers.GetValidInput("Enter Recovery Model Option (1, 2, or 3): ", "Recovery Model cannot be empty. Please enter a valid Recovery model option (1, 2, or 3).");
                switch (recoveryModelInput)
                {
                    case "1":
                        recoveryModel = DatabaseOperations.RecoveryModel.Simple;
                        break;
                    case "2":
                        recoveryModel = DatabaseOperations.RecoveryModel.Full;
                        break;
                    case "3":
                        recoveryModel = DatabaseOperations.RecoveryModel.BULK_LOGGED;
                        break;
                    default:
                        ConsoleHelpers.WriteLineColoredMessage(
                            "Invalid Recovery Model Option. Please enter a valid option (1, 2, or 3).", ConsoleColor.Red);
                        break;
                }
            }

            string confirmation;
            do
            {
                ConsoleHelpers.WriteLineColoredMessage($"You are about to change the recovery model of database '{databaseName}' to '{recoveryModel}'.", ConsoleColor.DarkRed);
                ConsoleHelpers.WriteLineColoredMessage("Do you want to continue? (Y/N)", ConsoleColor.DarkRed);
                confirmation = Console.ReadLine()?.Trim().ToUpper();
                if (confirmation == "Y")
                {


                    DatabaseOperations.ChangeRecoveryModel(Program.ConnectionString, databaseName, recoveryModel.Value);
                    ConsoleHelpers.WriteLineColoredMessage($"Recovery model of database '{databaseName}' has been changed to '{recoveryModel}'.", ConsoleColor.Green);
                }
                else if (confirmation == "N")
                {
                    ConsoleHelpers.WriteLineColoredMessage("Index rebuild operation cancelled.", ConsoleColor.DarkYellow);
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage("Invalid input. Please enter y or n.", ConsoleColor.Red);
                }
            } while (confirmation != "Y" && confirmation != "N");

        }
    }
}
