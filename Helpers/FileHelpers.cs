namespace MssqlToolBox.Helpers
{
    internal static class FileHelpers
    {
        public static string CreateAndGetOutputPath(string server)
        {
            var basePath = "RebuildIndexOutput";
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            if (server == ".")
                server = "localhost";
            var serverWithoutPort = server.Contains(',') ? server.Split(',')[0] : server;
            if (!Directory.Exists(Path.Combine(basePath, serverWithoutPort)))
                Directory.CreateDirectory(Path.Combine(basePath, serverWithoutPort));

            var outputPath = Path.Combine(basePath, serverWithoutPort, DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            return outputPath;
        }
    }
}
