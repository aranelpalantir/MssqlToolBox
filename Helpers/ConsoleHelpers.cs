namespace MssqlToolBox.Helpers
{
    internal static class ConsoleHelpers
    {
        public static string GetValidInput(string prompt, string errorMessage)
        {
            string input;
            do
            {
                WriteColoredMessage(prompt, ConsoleColor.DarkCyan);
                input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    WriteLineColoredMessage(errorMessage, ConsoleColor.Red);
                }
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }
        public static string GetValidInputWithDefaultInput(string prompt, string defaultInput)
        {
            WriteColoredMessage(prompt, ConsoleColor.DarkCyan);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultInput;
            }

            return input;
        }
        public static string ReadPassword(string prompt, string errorMessage)
        {
            string password;
            do
            {
                WriteColoredMessage(prompt, ConsoleColor.DarkCyan);
                password = ReadPassword();

                if (string.IsNullOrWhiteSpace(password))
                {
                    WriteLineColoredMessage(errorMessage, ConsoleColor.Red);
                }
            } while (string.IsNullOrWhiteSpace(password));

            return password;
        }

        private static string ReadPassword()
        {
            var password = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    WriteColoredMessage("*", ConsoleColor.Yellow);
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        public static void WriteLineColoredMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteColoredMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }

        public static bool ConfirmAction(string message)
        {
            do
            {
                WriteLineColoredMessage(message, ConsoleColor.DarkRed);
                var confirmation = Console.ReadLine()?.Trim().ToUpper();
                if (confirmation == "Y" || confirmation == "N")
                {
                    return confirmation == "Y";
                }
                else
                {
                    WriteLineColoredMessage("Invalid input. Please enter Y or N.", ConsoleColor.Red);
                }
            } while (true);
        }

        public static int GetFragmentationLimit()
        {
            return int.Parse(GetValidInput("Fragmentation Limit (0 for all indexes): ",
                "Fragmentation Limit cannot be empty. Please enter a valid Fragmentation limit."));
        }

        public static string? SelectTable(string databaseName)
        {
            var availableTables = DatabaseOperations.GetTables(databaseName);
            return SelectItem("Table", availableTables);
        }

        public static string? SelectDatabase()
        {
            var onlineDatabases = DatabaseOperations.GetOnlineDatabases();
            return SelectItem("Database", onlineDatabases);
        }
        public static string? SelectDatabaseWithRecoveryModel()
        {
            var onlineDatabasesWithRecoveryModels = DatabaseOperations.GetOnlineDatabasesWithRecoveryModels();
            return SelectItem("Database", onlineDatabasesWithRecoveryModels);
        }

        private static string? SelectItem(string itemType, List<string> itemList)
        {
            if (itemList.Count == 0)
            {
                WriteLineColoredMessage($"There are no available {itemType.ToLower()}s.", ConsoleColor.DarkYellow);
                return null;
            }
            else
            {
                WriteLineColoredMessage($"Available {itemType}s:", ConsoleColor.Yellow);
                for (var i = 0; i < itemList.Count; i++)
                {
                    WriteLineColoredMessage($"{i + 1}. {itemList[i]}", ConsoleColor.Cyan);
                }

                while (true)
                {
                    var itemIndexInput = GetValidInput($"Enter the {itemType.ToLower()} number (* for all {itemType.ToLower()}s): ",
                        $"{itemType} number cannot be empty. Please enter a valid {itemType.ToLower()} number.");

                    if (itemIndexInput == "*")
                    {
                        return "*";
                    }

                    if (int.TryParse(itemIndexInput, out var itemIndex) && itemIndex > 0 && itemIndex <= itemList.Count)
                    {
                        return itemList[itemIndex - 1];
                    }
                    else
                    {
                        WriteLineColoredMessage($"Invalid {itemType.ToLower()} number. Please enter a valid {itemType.ToLower()} number.", ConsoleColor.Red);
                    }
                }
            }
        }
    }
}

