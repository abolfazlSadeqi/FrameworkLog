using FrameworkLog.Models.Base;
using Serilog.Events;

namespace FrameworkLog.Models.Others;

public class LoggingConfig
{
    public string ApplicationName { get; set; }
    public string ApplicationVersion { get; set; }
    public string Environment { get; set; } = "Development";
    public string TimeZone { get; set; } = "UTC";
    public bool EnableGlobalMetadata { get; set; } = true;
    public List<string> GlobalTags { get; set; } = new();
    public LoggingOverrideConfig OverrideSettings { get; set; } = new();


    public Dictionary<LogLevelType, LogLevelSettings> PerLevelSettings { get; set; } = new();
}


public class LoggingOverrideConfig
{
    public bool Enabled { get; set; } = false; 
    public LogEventLevel MicrosoftLevel { get; set; } = LogEventLevel.Warning;
    public LogEventLevel SystemLevel { get; set; } = LogEventLevel.Warning;
}

