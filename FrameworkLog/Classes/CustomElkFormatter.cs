
using FrameworkLog.Models.Others;
using Serilog.Events;
using Serilog.Formatting;
using System.IO;
using System.Text.Json;

namespace FrameworkLog.Classes;


public class CustomElkFormatter : ITextFormatter
{
    private readonly OutputTemplateConfig _config;

    public CustomElkFormatter(OutputTemplateConfig config)
    {
        _config = config;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var logData = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(_config.CustomTemplate))
        {
            if (_config.CustomTemplate == "{Message}")
            {
                output.WriteLine(logEvent.RenderMessage());
                return;
            }
        }

        //  DefaultFields
        if (_config.DefaultFields.Any())
        {
            foreach (var field in _config.DefaultFields)
            {
                switch (field)
                {
                    case "Timestamp":
                        logData["Timestamp"] = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
                        break;
                    case "Level":
                        logData["Level"] = logEvent.Level.ToString();
                        break;
                    case "Message":
                        logData["Message"] = logEvent.RenderMessage();
                        break;
                    case "Exception":
                        if (logEvent.Exception != null)
                            logData["Exception"] = logEvent.Exception.ToString();
                        break;
                    default:
                        if (logEvent.Properties.ContainsKey(field))
                            logData[field] = logEvent.Properties[field].ToString();
                        break;
                }
            }
        }
        else
        {
            logData["Message"] = logEvent.RenderMessage();
        }

        output.WriteLine(JsonSerializer.Serialize(logData));
    }
}
