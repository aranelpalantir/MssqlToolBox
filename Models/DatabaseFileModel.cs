namespace MssqlToolBox.Models
{
    internal record DatabaseFileModel
    {
        public string Name { get; set; }
        public decimal Size { get; set; }
    }
}
