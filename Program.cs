using MssqlToolBox.Helpers;
namespace MssqlToolBox
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            DatabaseCredentialsHandler.Handle();
        }
    }
}
