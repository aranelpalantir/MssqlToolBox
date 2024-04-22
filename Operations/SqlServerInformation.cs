using MssqlToolBox.Helpers;
using MssqlToolBox.Models;

namespace MssqlToolBox.Operations
{
    internal static class SqlServerInformation
    {
        private static void ShowServerStatus(ServerStatusModel serverStatusModel)
        {
            ConsoleHelpers.WriteLineColoredMessage($"SQL Server: {CredentialManager.Instance.GetActiveConnection().Server} Start Time: {serverStatusModel.SqlServerStartTime} User: {CredentialManager.Instance.GetActiveConnection().Username}", ConsoleColor.DarkCyan);
            ConsoleHelpers.WriteLineColoredMessage("-----------------------------------------------------------------------------------------------------------------------", ConsoleColor.Gray);
            ConsoleHelpers.WriteLineColoredMessage("Ram Information", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLineColoredMessage("-----------------------------------------------------------------------------------------------------------------------", ConsoleColor.Gray);
            ConsoleHelpers.WriteColoredMessage("RAM Size: ", ConsoleColor.DarkYellow);
            Console.Write($"{serverStatusModel.RamSizeMB,-6} MB |");
            ConsoleHelpers.WriteColoredMessage(" Used RAM: ", ConsoleColor.Yellow);
            Console.Write($"{serverStatusModel.UsedRamSizeMB,-6} MB |");
            ConsoleHelpers.WriteColoredMessage(" Free RAM: ", ConsoleColor.Green);
            Console.Write($"{serverStatusModel.FreeRamSizeMB,-6} MB |");
            ConsoleHelpers.WriteColoredMessage(" RAM Usage Percentage: ", ConsoleColor.Blue);
            Console.Write($"{serverStatusModel.RamUsagePercentage,-4}% |");
            ConsoleHelpers.WriteLineColoredMessage($" High Memory Usage: {(serverStatusModel.IsHighMemoryUsage ? "Yes" : "No")}", serverStatusModel.IsHighMemoryUsage ? ConsoleColor.Red : ConsoleColor.Green);
            ConsoleHelpers.WriteLineColoredMessage("-----------------------------------------------------------------------------------------------------------------------", ConsoleColor.Gray);
        }

        private static void ShowDriveInformation(List<Models.DriveInfo> driveInfos)
        {
            ConsoleHelpers.WriteLineColoredMessage("Drive Free Space Information", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLineColoredMessage("-----------------------------------------------------------------------------------------------------------------------", ConsoleColor.Gray);
            foreach (var driveInfo in driveInfos)
            {
                ConsoleHelpers.WriteColoredMessage($"{driveInfo.DriveLetter}: {driveInfo.FreeSpaceMB} MB ", ConsoleColor.DarkYellow);
            }

            Console.WriteLine("");
        }

        public static void ShowSummaryAllConnections()
        {
            var activeConnection = CredentialManager.Instance.GetActiveConnection();
            Console.Clear();
            var count = 1;
            foreach (var databaseConnectionInfo in CredentialManager.Instance.GetConnections())
            {
                CredentialManager.Instance.SetActiveConnection(databaseConnectionInfo);
                ConsoleHelpers.WriteColoredMessage($"{count++}. ", ConsoleColor.DarkYellow);
                ServerSummary(DatabaseOperations.GetServerStatus());
                ConsoleHelpers.WriteLineColoredMessage("-----------------------------------------------------------------------------------------------------------------------", ConsoleColor.Gray);
            }
            CredentialManager.Instance.SetActiveConnection(activeConnection);
        }
        public static void ServerSummary(ServerStatusModel serverStatusModel)
        {
            ShowServerStatus(serverStatusModel);
            ShowDriveInformation(serverStatusModel.DriveInfos);
        }
    }
}
