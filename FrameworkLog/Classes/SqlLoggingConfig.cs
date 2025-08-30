namespace FrameworkLog.Classes;

public class SqlLoggingConfig
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }
    public string TableName { get; set; } = "Logs";
    public int BatchSize { get; set; } = 50;
    public bool EnableFallbackToFile { get; set; }
}



