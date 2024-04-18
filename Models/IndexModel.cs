namespace MssqlToolBox.Models
{
    internal record IndexModel
    {
        public string DatabaseName { get; set; }
        public string TableName { get; set; }
        public string Name { get; set; }
        public double Fragmentation { get; set; }
    }
}
