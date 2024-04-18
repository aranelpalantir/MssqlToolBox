using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class ReorganizeIndexes
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
            var limit = int.Parse(ConsoleHelpers.GetValidInput("Fragmentation Limit (0 for all indexes): ", "Fragmentation Limit cannot be empty. Please enter a valid Fragmentation limit."));
            string confirmation;
            do
            {
                ConsoleHelpers.WriteLineColoredMessage("Are you sure you want to reorganize indexes? (Y/N)", ConsoleColor.DarkRed);
                confirmation = Console.ReadLine()?.Trim().ToUpper();
                if (confirmation == "Y")
                {
                    var indexes = DatabaseOperations.GetIndexFragmentations(Program.ConnectionString, databaseName, tableName);

                    foreach (var index in indexes)
                    {
                        if (index.Fragmentation >= limit)
                        {
                            DatabaseOperations.ReorganizeIndex(Program.ConnectionString, index.DatabaseName,
                                index.TableName, index.Name);
                            var newFragmentation = DatabaseOperations.GetIndexFragmentation(Program.ConnectionString,
                                index.DatabaseName, index.TableName, index.Name);
                            ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName}=>{index.TableName}.{index.Name}: Index reorganize operation is OK. (Fragmentation: {index.Fragmentation} to {newFragmentation})", ConsoleColor.Green);
                        }
                        else
                        {
                            ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName}=>{index.TableName}.{index.Name}: Index reorganize operation is not necessary. (Fragmentation: {index.Fragmentation})", ConsoleColor.DarkYellow);
                        }
                    }
                    ConsoleHelpers.WriteLineColoredMessage("Index reorganize operation completed successfully.", ConsoleColor.Green);
                }
                else if (confirmation == "N")
                {
                    ConsoleHelpers.WriteLineColoredMessage("Index reorganize operation cancelled.", ConsoleColor.DarkYellow);
                }
                else
                {
                    ConsoleHelpers.WriteLineColoredMessage("Invalid input. Please enter Y or N.", ConsoleColor.Red);
                }
            } while (confirmation != "Y" && confirmation != "N");
        }
    }
}
