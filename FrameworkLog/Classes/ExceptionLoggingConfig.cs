namespace FrameworkLog.Classes;

public class ExceptionLoggingConfig
{
    public bool Enabled { get; set; }
    public bool IncludeStackTrace { get; set; } = true;
    public bool IncludeInnerExceptions { get; set; } = true;
}



