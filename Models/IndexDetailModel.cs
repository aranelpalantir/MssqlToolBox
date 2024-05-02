namespace MssqlToolBox.Models
{
    internal record IndexDetailModel
    {
        public string TableName { get; set; }
        public string Name { get; set; }
        public string IndexType { get; set; }
        public string ColumnNames { get; set; }
        public string IncludedColumnNames { get; set; }
      
    }
}
