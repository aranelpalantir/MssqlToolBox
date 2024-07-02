using Microsoft.Data.SqlClient;

namespace MssqlToolBox.Models
{
    internal record DatabaseConnectionInfo(string Server, string Username, string Password)
    {
        public string Server { get; set; } = Server;
        public string Username { get; set; } = Username;
        public string Password { get; set; } = Password;
        public string ConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    UserID = Username,
                    Password = Password,
                    TrustServerCertificate = true
                };

                return builder.ConnectionString;
            }
        }
    }
}
