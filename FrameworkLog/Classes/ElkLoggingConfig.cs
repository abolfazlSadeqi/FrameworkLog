namespace FrameworkLog.Classes;

public class ElkLoggingConfig
{
    public bool Enabled { get; set; }
    public string Url { get; set; }
    public string IndexPattern { get; set; } = "logs-{level}-{yyyy.MM.dd}";
    public bool AutoRegisterTemplate { get; set; } = true;
    public int BatchSize { get; set; } = 100;
}


