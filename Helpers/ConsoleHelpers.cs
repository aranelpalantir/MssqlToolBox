namespace MssqlToolBox.Helpers
{
    internal static class ConsoleHelpers
    {
        public static string GetValidInput(string prompt, string errorMessage)
        {
            string input;
            do
            {
                WriteColoredMessage(prompt,ConsoleColor.DarkCyan);
                input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    WriteLineColoredMessage(errorMessage,ConsoleColor.Red);
                }
            } while (string.IsNullOrWhiteSpace(input));

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
    }
}
