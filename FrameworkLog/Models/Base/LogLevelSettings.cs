using FrameworkLog.Models.Others;
using FrameworkLog.Models.Rotate_Archive;
using FrameworkLog.Models.Sink;

namespace FrameworkLog.Models.Base;

public class LogLevelSettings
{
    public bool Enabled { get; set; } = true;

    // File Logging
    public FileLoggingConfig FileLogging { get; set; }
    public SqlLoggingConfig SqlLogging { get; set; }
    public ElkLoggingConfig ElkLogging { get; set; }
    
    public EnricherConfig Enrichers { get; set; }

    
    public LogControlConfig Control { get; set; }
    public PerformanceConfig Performance { get; set; }
    public ArchivingConfig Archiving { get; set; }
    public LogRotateConfig LogRotate { get; set; }


}



