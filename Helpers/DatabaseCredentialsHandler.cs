using MssqlToolBox.Models;

namespace MssqlToolBox.Helpers
{
    internal static class DatabaseCredentialsHandler
    {
        public static void Handle()
        {
            ConsoleHelpers.WriteLineColoredMessage("===== Microsoft SQL Server Tool Box Utility =====", ConsoleColor.DarkGreen);
            Console.WriteLine("");

            GetDatabaseCredentials();

            MenuHandler.Handle();
        }
        private static bool TestDatabaseCredentials()
        {
            try
            {
                if (DatabaseOperations.TestDatabaseConnection())
                {
                    ConsoleHelpers.WriteLineColoredMessage("Connected to the database successfully.", ConsoleColor.Green);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteLineColoredMessage($"Failed to connect to the database. Please check your connection information and try again: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }

        private static void GetDatabaseCredentials()
        {
            var newConnection = false;
            while (true)
            {
                if (CredentialManager.Instance.GetAvailableConnectionCount() > 0)
                {
                    CredentialManager.Instance.ListConnections();

                    Console.WriteLine("");
                    string choice;
                    while (true)
                    {
                        choice = ConsoleHelpers.GetValidInput(
                            "Would you like to use an existing connection (Y) or enter new credentials (N)? ",
                            "Please enter Y or N.");
                        if (choice.ToUpper() == "Y" || choice.ToUpper() == "N")
                            break;
                    }

                    if (choice.ToUpper() == "Y")
                    {
                        int index;
                        while (true)
                        {
                            var input = ConsoleHelpers.GetValidInput("Enter the number of the existing connection: ",
                                "Invalid input. Please enter a valid number.");
                            if (int.TryParse(input, out index) && index > 0 &&
                                index <= CredentialManager.Instance.GetAvailableConnectionCount())
                            {
                                break;
                            }

                            ConsoleHelpers.WriteLineColoredMessage(
                                "Invalid connection number. Please enter a valid number.", ConsoleColor.Red);
                        }

                        var connections = CredentialManager.Instance.GetConnections();
                        var connection = connections[index - 1];
                        CredentialManager.Instance.SetActiveConnection(connection);
                    }
                    else
                    {
                        newConnection = true;
                        GetNewDatabaseCredentials();
                    }
                }
                else
                {
                    newConnection = true;
                    GetNewDatabaseCredentials();
                }

                if (TestDatabaseCredentials())
                {
                    if (newConnection)
                    {
                        ConfirmSaveDatabaseCredentials(CredentialManager.Instance.GetActiveConnection());
                    }
                    break;
                }
            }
        }

        private static void GetNewDatabaseCredentials()
        {
            ConsoleHelpers.WriteLineColoredMessage("Please enter the database credentials:", ConsoleColor.DarkYellow);
            Console.WriteLine("");

            var server = ConsoleHelpers.GetValidInput("SQL Server: ", "SQL Server name cannot be empty. Please enter a valid SQL Server name.");
            var username = ConsoleHelpers.GetValidInput("Username: ", "Username cannot be empty. Please enter a valid username.");
            var password = ConsoleHelpers.ReadPassword("Password: ", "Password cannot be empty. Please enter a valid password.");

            var connection = new DatabaseConnectionInfo(server, username, password);
            CredentialManager.Instance.SetActiveConnection(connection);
        }

        private static void ConfirmSaveDatabaseCredentials(DatabaseConnectionInfo connection)
        {
            string choice;
            while (true)
            {
                choice = ConsoleHelpers.GetValidInput("Would you like to save this connection for future use? (Y/N): ", "Please enter Y or N.");
                if (choice.ToUpper() == "Y" || choice.ToUpper() == "N")
                    break;
            }

            if (choice.ToUpper() == "Y")
            {
                var defaultConnectionName = $"{connection.Server}_{connection.Username}";
                var connectionName = ConsoleHelpers.GetValidInputWithDefaultInput($"Enter a name for this connection (Default: {defaultConnectionName}): ", defaultConnectionName);
                CredentialManager.Instance.AddConnection(connectionName, connection);
            }
        }

    }
}
