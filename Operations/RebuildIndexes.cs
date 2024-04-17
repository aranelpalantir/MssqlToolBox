using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class RebuildIndexes
    {
        public static void Execute()
        {
            var outputPath = FileHelpers.CreateAndGetOutputPath(Program.Server);
            var databases = DatabaseOperations.GetOnlineDatabases(Program.ConnectionString);
            foreach (var dbName in databases)
            {
                var sqlCommand = $"sqlcmd -S {Program.Server} -d {dbName} -U {Program.Username} -P {Program.Password} -i SqlScripts\\RebuildIndex.sql -o {outputPath}\\{dbName}.txt";
                Console.WriteLine($"Executing SQL script for {dbName}");
                System.Diagnostics.Process.Start("CMD.exe", $"/C {sqlCommand}");
            }
            ConsoleHelpers.WriteLineColoredMessage("Index rebuild operation completed successfully.", ConsoleColor.Green);
        }
    }
}
