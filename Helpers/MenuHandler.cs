using MssqlToolBox.Models;
using MssqlToolBox.Operations;

namespace MssqlToolBox.Helpers
{
    internal static class MenuHandler
    {
        public static void Handle()
        {
            while (true)
            {
                try
                {
                    ShowMenu();

                    ConsoleHelpers.WriteColoredMessage("Make your choice: ", ConsoleColor.Green);
                    var choice = Console.ReadLine();

                    Console.Clear();

                    ExecuteMenuOption(choice);

                }
                catch (Exception ex)
                {
                    ConsoleHelpers.WriteLineColoredMessage("Error: " + ex.Message, ConsoleColor.Red);
                }
                finally
                {
                    ConsoleHelpers.WriteLineColoredMessage("Please press any key to return to the menu", ConsoleColor.DarkGray);
                    Console.ReadKey();
                }
            }
        }
        private static readonly Dictionary<string, MenuOption> MenuOptions = new()
        {
            { "1", new MenuOption(" List Online Databases", _ => ListOnlineDatabases.Execute()) },
            { "2", new MenuOption(" List Offline Databases", _ => ListOfflineDatabases.Execute()) },
            { "3", new MenuOption(" List Recovery Models", _ =>ListRecoveryModels.Execute()) },
            { "4", new MenuOption(" Change Recovery Model", _ =>ChangeRecoveryModel.Execute()) },
            { "5", new MenuOption(" List Index Fragmentations", _ =>ListIndexFragmentations.Execute()) },
            { "6", new MenuOption(" Rebuild Indexes", type => IndexOperation.Execute(IndexOperation.OperationType.Rebuild)) },
            { "7", new MenuOption(" Reorganize Indexes", _ =>IndexOperation.Execute(IndexOperation.OperationType.Reorganize)) },
            { "8", new MenuOption(" Update Index Statistics", _ =>IndexOperation.Execute(IndexOperation.OperationType.UpdateStatistics)) },
            { "9", new MenuOption(" Index Optimization", _ =>IndexOperation.Execute(IndexOperation.OperationType.Optimization)) },
            { "10", new MenuOption("Top 10 Queries by Avg. CPU Time", _ => ShowTopQueries.Execute(DatabaseOperations.ShowTopQueriesSortBy.CpuTime)) },
            { "11", new MenuOption("Top 10 Queries by Avg. Elapsed Time", _ => ShowTopQueries.Execute(DatabaseOperations.ShowTopQueriesSortBy.ElapsedTime)) },
            { "12", new MenuOption("Top 10 Active Queries by CPU Time", _ => ShowTopActiveQueries.Execute()) },
            { "13", new MenuOption("Top 10 Missing Indexes by Improvement Measure", _ => ListMissingIndexes.Execute()) },
            { "c", new MenuOption(" Change Sql Server Connection", _ =>  DatabaseCredentialsHandler.Handle()) },
            { "q", new MenuOption(" Exit", _ => Environment.Exit(0)) }
        };
        private static void ShowServerStatus(ServerStatusModel serverStatusModel)
        {
            ConsoleHelpers.WriteLineColoredMessage("Status", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);
            ConsoleHelpers.WriteColoredMessage($"RAM Size: {serverStatusModel.RamSizeMB} MB  ", ConsoleColor.DarkYellow);
            ConsoleHelpers.WriteColoredMessage($"Used RAM: {serverStatusModel.UsedRamSizeMB} MB  ", ConsoleColor.Yellow);
            ConsoleHelpers.WriteColoredMessage($"Free RAM: {serverStatusModel.FreeRamSizeMB} MB  ", ConsoleColor.Green);
            ConsoleHelpers.WriteLineColoredMessage($"RAM Usage Percentage: {serverStatusModel.RamUsagePercentage}%", ConsoleColor.Blue);
            ConsoleHelpers.WriteLineColoredMessage($"SQL Server Start Time: {serverStatusModel.SqlServerStartTime}", ConsoleColor.Magenta);
        }

        private static void ShowDriveInformation(List<Models.DriveInfo> driveInfos)
        {
            ConsoleHelpers.WriteLineColoredMessage("\nDrive Information", ConsoleColor.Cyan);
            ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);
            foreach (var driveInfo in driveInfos)
            {
                ConsoleHelpers.WriteColoredMessage($"Drive: {driveInfo.DriveLetter}, Free Space: {driveInfo.FreeSpaceMB} MB  ", ConsoleColor.DarkCyan);
            }

            Console.WriteLine("");
        }
        private static void ShowMenu()
        {
            Console.Clear();
            ConsoleHelpers.WriteLineColoredMessage($"SQL Server: {CredentialManager.Instance.GetActiveConnection().Server} User: {CredentialManager.Instance.GetActiveConnection().Username}", ConsoleColor.DarkCyan);

            var serverStatusModel = DatabaseOperations.GetServerStatus();
            ShowServerStatus(serverStatusModel);
            ShowDriveInformation(serverStatusModel.DriveInfos);

            ConsoleHelpers.WriteLineColoredMessage("-------------", ConsoleColor.Gray);
            ConsoleHelpers.WriteLineColoredMessage("Menu:", ConsoleColor.DarkYellow);
            foreach (var option in MenuOptions)
            {
                ConsoleHelpers.WriteLineColoredMessage($"{option.Key}- {option.Value.Description}", ConsoleColor.Yellow);
            }
        }

        private static void ExecuteMenuOption(string choice, object parameter = null)
        {
            if (MenuOptions.TryGetValue(choice, out var selectedOption))
            {
                selectedOption.Action.Invoke(parameter);
            }
            else
            {
                ConsoleHelpers.WriteLineColoredMessage("Invalid choice. Please try again.", ConsoleColor.Red);
            }
        }

        private sealed class MenuOption(string description, Action<object> action)
        {
            public string Description { get; set; } = description;
            public Action<object> Action { get; } = action;
        }
    }
}