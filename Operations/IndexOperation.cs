using MssqlToolBox.Helpers;

namespace MssqlToolBox.Operations
{
    internal static class IndexOperation
    {
        public enum OperationType
        {
            Rebuild,
            Reorganize,
            UpdateStatistics,
            Optimization
        }
        private static string GetOperationName(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Rebuild => "rebuild",
                OperationType.Reorganize => "reorganize",
                OperationType.UpdateStatistics => "update statistics",
                OperationType.Optimization => "rebuild, reorganize and update index statistics",
                _ => throw new ArgumentException("Invalid operation type"),
            };
        }
        public static void Execute(OperationType operationType)
        {
            var operationName = GetOperationName(operationType);

            var databaseName = ConsoleHelpers.SelectDatabase();
            if (databaseName == null)
            {
                ConsoleHelpers.WriteLineColoredMessage("Database selection cancelled or invalid. Index operation aborted.", ConsoleColor.DarkYellow);
                return;
            }

            var tableName = "*";
            if (databaseName != "*")
            {
                tableName = ConsoleHelpers.SelectTable(databaseName);
                if (tableName == null)
                {
                    ConsoleHelpers.WriteLineColoredMessage("Table selection cancelled or invalid. Index operation aborted.", ConsoleColor.DarkYellow);
                    return;
                }
            }

            var limit = ConsoleHelpers.GetFragmentationLimit();

            if (ConsoleHelpers.ConfirmAction($"Are you sure you want to {operationName} indexes? (Y/N)"))
            {
                ExecuteIndexOperation(operationType, databaseName, tableName, limit, operationName);
            }
            else
            {
                ConsoleHelpers.WriteLineColoredMessage("Index operation cancelled.", ConsoleColor.DarkYellow);
            }
        }

        private static void ExecuteIndexOperation(OperationType operationType, string databaseName, string tableName, int limit, string operationName)
        {
            var indexes = DatabaseOperations.GetIndexFragmentations(Program.ConnectionString, databaseName, tableName);

            foreach (var index in indexes)
            {
                switch (operationType)
                {
                    case OperationType.Rebuild or OperationType.Reorganize when index.Fragmentation >= limit:
                        {
                            if (operationType == OperationType.Rebuild)
                                DatabaseOperations.RebuildIndex(Program.ConnectionString,
                                    index.DatabaseName,
                                    index.TableName, index.Name);
                            else
                                DatabaseOperations.ReorganizeIndex(Program.ConnectionString,
                                    index.DatabaseName,
                                    index.TableName, index.Name);

                            var newFragmentation = DatabaseOperations.GetIndexFragmentation(Program.ConnectionString,
                                index.DatabaseName, index.TableName, index.Name);
                            ConsoleHelpers.WriteLineColoredMessage(
                                $"{index.DatabaseName} => {index.TableName}.{index.Name}: Index {operationName} operation is OK. (Fragmentation: {index.Fragmentation} to {newFragmentation})",
                                ConsoleColor.Green);
                            break;
                        }
                    case OperationType.Rebuild or OperationType.Reorganize:
                        ConsoleHelpers.WriteLineColoredMessage(
                            $"{index.DatabaseName}=>{index.TableName}.{index.Name}: Index {operationName} operation is not necessary. (Fragmentation: {index.Fragmentation})",
                            ConsoleColor.DarkYellow);
                        break;
                    case OperationType.UpdateStatistics:
                        DatabaseOperations.UpdateStatistics(Program.ConnectionString, index.DatabaseName,
                            index.TableName, index.Name);

                        ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName}=>{index.TableName}.{index.Name}: Index {operationName} operation is OK.", ConsoleColor.Green);
                        break;
                    case OperationType.Optimization:
                        {
                            var indexOperation = false;
                            if (index.Fragmentation >= limit)
                            {
                                DatabaseOperations.RebuildIndex(Program.ConnectionString, index.DatabaseName,
                                    index.TableName, index.Name);
                                DatabaseOperations.ReorganizeIndex(Program.ConnectionString, index.DatabaseName,
                                    index.TableName, index.Name);
                                var newFragmentation = DatabaseOperations.GetIndexFragmentation(Program.ConnectionString,
                                    index.DatabaseName, index.TableName, index.Name);
                                ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName}=>{index.TableName}.{index.Name}: Index {operationName} is OK. (Fragmentation: {index.Fragmentation} to {newFragmentation})", ConsoleColor.Green);
                                indexOperation = true;
                            }

                            DatabaseOperations.UpdateStatistics(Program.ConnectionString, index.DatabaseName,
                                index.TableName, index.Name);

                            if (!indexOperation)
                                ConsoleHelpers.WriteLineColoredMessage($"{index.DatabaseName} => {index.TableName}.{index.Name}: Index rebuild, reorganize is not necessary. Only updated index statistics (Fragmentation: {index.Fragmentation})", ConsoleColor.DarkYellow);
                            break;
                        }
                }
            }
            ConsoleHelpers.WriteLineColoredMessage($"Index {operationName} operation completed successfully.", ConsoleColor.Green);
        }
    }

}
