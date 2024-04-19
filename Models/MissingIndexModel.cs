namespace MssqlToolBox.Models
{
    internal record MissingIndexModel
    {
        public string DatabaseName { get; init; }
        public decimal ImprovementMeasure { get; init; }
        public string CreateIndexStatement { get; init; }
    }

}
