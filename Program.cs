using MssqlToolBox.Helpers;
namespace MssqlToolBox
{
    internal static class Program
    {
        public static string Server = string.Empty;
        public static string ConnectionString = string.Empty;

        static void Main(string[] args)
        {
            ConsoleHelpers.WriteLineColoredMessage("===== Microsoft SQL Server Tool Box Utility =====", ConsoleColor.DarkGreen);
            Console.WriteLine("");
            ConsoleHelpers.WriteLineColoredMessage("Please enter the database credentials:", ConsoleColor.DarkYellow);
            Console.WriteLine("");

            DatabaseCredentialsHandler.Handle();

            MenuHandler.Handle();
        }

    }
}
