using MssqlToolBox.Helpers;
namespace MssqlToolBox
{
    internal static class Program
    {
        public static string Server = string.Empty;
        public static string ConnectionString = string.Empty;

        static void Main(string[] args)
        {
            DatabaseCredentialsHandler.Handle();
        }
    }
}
