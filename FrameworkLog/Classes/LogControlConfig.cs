namespace FrameworkLog.Classes;

public class LogControlConfig
{
    public List<string> DisabledTags { get; set; } = new();
    public Dictionary<string, LogLevelType> MinLevelPerTag { get; set; } = new();
}



