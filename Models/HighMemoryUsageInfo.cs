namespace MssqlToolBox.Models
{
    internal record HighMemoryUsageInfo
    {
        public int PleValue { get; set; }
        public int PleLimit { get; set; }
        public bool IsHighMemoryUsage { get; set; }
    }
}
