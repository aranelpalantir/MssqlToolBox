namespace MssqlToolBox.Models
{
    internal record IndexUsageStatisticModel
    {
        public string TableName { get; set; }
        public string Name { get; set; }
        public string IndexType { get; set; }
        public int Seeks { get; set; }
        public int Scans { get; set; }
        public int Lookups { get; set; }
        public int Updates { get; set; }
    }
}
