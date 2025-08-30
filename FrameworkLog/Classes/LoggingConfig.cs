namespace FrameworkLog.Classes;

public class LoggingConfig
{
    public string ApplicationName { get; set; }
    public string ApplicationVersion { get; set; }
    public string Environment { get; set; } = "Development";
    public string TimeZone { get; set; } = "UTC";
    public bool EnableGlobalMetadata { get; set; } = true;
    public List<string> GlobalTags { get; set; } = new();

    // تنظیمات اختصاصی برای هر سطح
    public Dictionary<LogLevelType, LogLevelSettings> PerLevelSettings { get; set; } = new();
}



