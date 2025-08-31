namespace FrameworkLog.Models.Others;

public class PerformanceConfig
{
    public bool Enabled { get; set; } = true;

    public int BatchSize { get; set; } = 50;
    public int FlushIntervalMs { get; set; } = 2000;
    public bool AsyncLogging { get; set; } = true;
}



