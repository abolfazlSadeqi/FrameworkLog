using FrameworkLog.Models.Base;

namespace FrameworkLog.Models.Rotate_Archive;

public class LogControlConfig
{
    public List<string> DisabledTags { get; set; } = new();
    public Dictionary<string, LogLevelType> MinLevelPerTag { get; set; } = new();
}



