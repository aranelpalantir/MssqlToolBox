namespace MssqlToolBox.Models
{
    internal record ServerStatusModel
    {
        public double RamSizeMB { get; set; }
        public double UsedRamSizeMB { get; set; }
        public double FreeRamSizeMB { get; set; }
        public double RamUsagePercentage { get; set; }
        public DateTime SqlServerStartTime { get; set; }
        public List<DriveInfo> DriveInfos { get; set; }
        public bool IsHighMemoryUsage { get; set; }
    }
    internal record DriveInfo
    {
        public string DriveLetter { get; set; }
        public double FreeSpaceMB { get; set; }
    }
}
