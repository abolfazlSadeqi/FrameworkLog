namespace FrameworkLog.Classes;

public class LogLevelSettings
{
    public bool Enabled { get; set; } = true;

    // File Logging
    public FileLoggingConfig FileLogging { get; set; }
    public SqlLoggingConfig SqlLogging { get; set; }
    public ElkLoggingConfig ElkLogging { get; set; }
    public OtherSinkConfig OtherSink { get; set; }

    public EnricherConfig Enrichers { get; set; }
    public RequestLoggingConfig RequestLogging { get; set; }
    public ExceptionLoggingConfig ExceptionLogging { get; set; }
    public MaskingConfig Masking { get; set; }
    public DetailedLoggingConfig DetailedLogging { get; set; }
    public LogControlConfig Control { get; set; }
    public PerformanceConfig Performance { get; set; }
    public ArchivingConfig Archiving { get; set; }
    public LogRotateConfig LogRotate { get; set; }
}



