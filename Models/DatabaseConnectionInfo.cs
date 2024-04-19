namespace MssqlToolBox.Models
{
    internal record DatabaseConnectionInfo(string Server, string Username, string Password)
    {
        public string Server { get; set; } = Server;
        public string Username { get; set; } = Username;
        public string Password { get; set; } = Password;
        public string ConnectionString => $"Data Source={Server};User ID={Username};Password={Password};TrustServerCertificate=true;";
    }
}
