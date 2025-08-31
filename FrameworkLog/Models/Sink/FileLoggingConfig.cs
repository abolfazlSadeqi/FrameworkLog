using FrameworkLog.Models.Others;

namespace FrameworkLog.Models.Sink;

public class FileLoggingConfig
{
    public bool Enabled { get; set; }
    public string Directory { get; set; }
    public string FileNamePattern { get; set; } = "log_{level}_{yyyyMMdd}.log";
    public string RollingInterval { get; set; } = "Day";
    public bool RollOnFileSizeLimit { get; set; }
    public long? FileSizeLimitBytes { get; set; }
    public int? MaxRetainedFiles { get; set; }
    public bool EnableCompression { get; set; }
    public string ArchivePath { get; set; }
    public OutputTemplateConfig OutputTemplate { get; set; }

}



