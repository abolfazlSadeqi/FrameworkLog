namespace FrameworkLog.Classes;

public class OtherSinkConfig
{
    public bool Enabled { get; set; }
    public string Type { get; set; } // Kafka, Sentry, RabbitMQ ...
    public string Destination { get; set; }
    public int BatchSize { get; set; }
}


