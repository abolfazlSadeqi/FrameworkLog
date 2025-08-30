namespace FrameworkLog.Classes;

public class RequestLoggingConfig
{
    public bool Enabled { get; set; }
    public bool IncludeHeaders { get; set; }
    public bool IncludeBody { get; set; }
    public bool MaskSensitiveData { get; set; }
}



