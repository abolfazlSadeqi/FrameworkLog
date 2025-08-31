using FrameworkLog.Models.Others;
using Serilog.Core;
using Serilog.Events;

namespace FrameworkLog.Classes;

// 📌 Enricher برای Http Request (Headers + Body)
public class HttpRequestEnricher : ILogEventEnricher
{
    private readonly RequestLoggingConfig _config;
    public HttpRequestEnricher(RequestLoggingConfig config) => _config = config;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_config.IncludeHeaders)
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestHeaders", "/* capture headers here */"));

        if (_config.IncludeBody)
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("RequestBody", "/* capture body here */"));
    }
}
