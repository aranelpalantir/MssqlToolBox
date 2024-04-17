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
            { "1", new MenuOption("List Online Databases", ListOnlineDatabases.Execute) },
            { "2", new MenuOption("List Offline Databases", ListOfflineDatabases.Execute) },
            { "3", new MenuOption("List Recovery Models", ListRecoveryModels.Execute) },
            { "4", new MenuOption("Change Recovery Model", ChangeRecoveryModel.Execute) },
            { "5", new MenuOption("List Index Fragmentations", ListIndexFragmentations.Execute) },
            { "6", new MenuOption("Rebuild Indexes of All Online Databases (Fragmentation Limit:5)", RebuildIndexes.Execute) },
            { "q", new MenuOption("Exit", () => Environment.Exit(0)) }
        };

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine($"SQL Server: {Program.Server}");
            Console.WriteLine("Menu:");
            foreach (var option in MenuOptions)
            {
                Console.WriteLine($"{option.Key}- {option.Value.Description}");
            }
        }

        private static void ExecuteMenuOption(string choice)
        {
            if (MenuOptions.TryGetValue(choice, out var selectedOption))
            {
                selectedOption.Action.Invoke();
            }
            else
            {
                ConsoleHelpers.WriteLineColoredMessage("Invalid choice. Please try again.", ConsoleColor.Red);
            }
        }
        private sealed class MenuOption(string description, Action action)
        {
            public string Description { get; set; } = description;
            public Action Action { get; } = action;
        }
    }
}