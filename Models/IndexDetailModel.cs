namespace MssqlToolBox.Models
{
    internal record IndexDetailModel
    {
        public string TableName { get; set; }
        public string Name { get; set; }
        public string IndexType { get; set; }
        public string ColumnNames { get; set; }
        public string IncludedColumnNames { get; set; }
        public double? Fragmentation { get; set; }
        public int? Seeks { get; set; }
        public int? Scans { get; set; }
        public int? Lookups { get; set; }
        public int? Updates { get; set; }
    }
}
