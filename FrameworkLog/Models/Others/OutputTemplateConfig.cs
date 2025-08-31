namespace FrameworkLog.Models.Others;

public class OutputTemplateConfig
{
    public bool UseDefaultTemplate { get; set; } = true;
    public string? CustomTemplate { get; set; }
    public List<string> DefaultFields { get; set; } = new(); // : Timestamp, Level, Message
}

