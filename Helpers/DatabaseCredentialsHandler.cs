namespace MssqlToolBox.Helpers
{
    internal static class DatabaseCredentialsHandler
    {
        public static void Handle()
        {
            while (true)
            {
                GetDatabaseCredentials();
                if (TestDatabaseCredentials())
                    break;
            }
        }
        private static bool TestDatabaseCredentials()
        {
            try
            {
                if (DatabaseOperations.TestDatabaseConnection(Program.ConnectionString))
                {
                    ConsoleHelpers.WriteLineColoredMessage("Connected to the database successfully.", ConsoleColor.Green);
                    ConsoleHelpers.WriteLineColoredMessage("Please press any key to go to the menu", ConsoleColor.DarkGray);
                    Console.ReadKey();
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
            Program.Server = ConsoleHelpers.GetValidInput("SQL Server: ", "SQL Server name cannot be empty. Please enter a valid SQL Server name.");
            Program.Username = ConsoleHelpers.GetValidInput("Username: ", "Username cannot be empty. Please enter a valid username.");
            Program.Password = ConsoleHelpers.ReadPassword("Password: ", "Password cannot be empty. Please enter a valid password.");

            Program.ConnectionString = $"Data Source={Program.Server};User ID={Program.Username};Password={Program.Password};TrustServerCertificate=true;";
        }
    }
}
