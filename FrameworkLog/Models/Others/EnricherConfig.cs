namespace FrameworkLog.Models.Others;

public class EnricherConfig
{
    public bool Enabled { get; set; }
    public bool EnableCorrelationId { get; set; }


    
    public List<string> ActiveEnrichers { get; set; } = new();
}



