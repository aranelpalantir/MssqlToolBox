namespace MssqlToolBox.Models
{
    internal record DatabaseModel
    {
        public string Name { get; set; }
        public string RecoveryModel { get; set; }
    }
}
