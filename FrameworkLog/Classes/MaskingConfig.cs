namespace FrameworkLog.Classes;

public class MaskingConfig
{
    public bool Enabled { get; set; }
    public List<string> SensitiveKeys { get; set; } = new();
    public string MaskReplacement { get; set; } = "*****";
}



