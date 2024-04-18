using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class UpdateIndexStatistics
    {
        public static void Execute()
        {
            var databaseName = ConsoleHelpers.GetValidInput("Database Name (* for all databases): ", "Database Name cannot be empty. Please enter a valid Database name.");
            var tableName = "*";
            if (databaseName != "*")
            {
                tableName = ConsoleHelpers.GetValidInput("Table Name (* for all tables): ",
                    "Table Name cannot be empty. Please enter a valid Table name.");
            }
            string confirmation;
            do
            {
                ConsoleHelpers.WriteLineColoredMessage("Are you sure you want to update index statistics? (Y/N)", ConsoleColor.DarkRed);
                confirmation = Console.ReadLine()?.Trim().ToUpper();
                if (confirmation == "Y")
                {
                    var indexes = DatabaseOperations.GetIndexFragmentations(Program.ConnectionString, databaseName, tableName);

                    foreach (var index in indexes)
                    {
                        DatabaseOperations.UpdateStatistics(Program.ConnectionString, index.DatabaseName,
                            index.TableName, index.Name);

                        ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName}=>{index.TableName}.{index.Name}: Update Index Statistics operation is OK.", ConsoleColor.Green);
                    }
                    ConsoleHelpers.WriteLineColoredMessage("Update Index Statistics operation completed successfully.", ConsoleColor.Green);
                }
                else if (confirmation == "N")
                {
                    ConsoleHelpers.WriteLineColoredMessage("Update Index Statistics operation cancelled.", ConsoleColor.DarkYellow);
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage("Invalid input. Please enter Y or N.", ConsoleColor.Red);
                }
            } while (confirmation != "Y" && confirmation != "N");
        }
    }
}
