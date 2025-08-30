namespace FrameworkLog.Classes;

public class EnricherConfig
{
    public bool Enabled { get; set; }
    public List<string> ActiveEnrichers { get; set; } = new();
}



