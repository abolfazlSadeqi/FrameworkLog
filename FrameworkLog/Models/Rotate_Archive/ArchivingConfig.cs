namespace FrameworkLog.Models.Rotate_Archive;

public class ArchivingConfig
{
    public bool Enabled { get; set; }
    public string ArchivePath { get; set; }
    public int RetentionDays { get; set; } = 30;
}



