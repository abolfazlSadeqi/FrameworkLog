namespace FrameworkLog.Classes;

public class LogRotateConfig
{
    public bool Enabled { get; set; }
    public string RotationFrequency { get; set; } = "Day";
    public int MaxFileSizeMB { get; set; } = 100;
    public int MaxRetainedFiles { get; set; } = 10;
}



