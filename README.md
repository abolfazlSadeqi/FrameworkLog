# FrameworkLog

🔹 One of the biggest challenges in projects has always been **logging complexity**.
Normally, you’d have to install multiple packages (for files, databases, ELK, metadata, etc.) and spend hours configuring them.

I built this library to make it **super simple and all-in-one**.
📦 Just install the package → add some simple config → done!

✨ **Key Features:**

* ⚡ **Unified & Simple Configuration** – no need for multiple packages.
* 📝 **Per-Level Configurations** – customize logging for each level (Info, Error, Warning, etc.).
* 🌍 **Multi-Sink Support** – log to multiple destinations at the same time (File, Database, ELK, etc.).
* 🧩 **Correlation ID Support** – trace requests end-to-end across distributed systems.
* 🎨 **Custom Output Templates** – design your log format exactly how you want, with structured fields & metadata.
* 🏷 **Tag-Based Control** – assign tags to logs and easily enable/disable them or set per-tag minimum levels.
* 🎛 **Log Control Policies** – fine-grained filtering without touching the source code.
* 🚀 **Performance Optimized** – async logging, batching, rotation & compression built-in.
* 📂 **Archiving & Retention** – automatic rotation, archiving, and retention policies.
* 🔌 **Extensible Design** – easily add new sinks (Kafka, Sentry, Console, etc.).
* 🔍 **Advanced Request & Exception Logging** – capture headers, bodies, stack traces, and inner exceptions.
* 🌐 **Global Metadata** – always include app name, version, environment, and global tags.

🎯 **The Goal:**
I designed this package so developers don’t have to waste time juggling multiple logging libraries and complex setups.

# How to Use

Install the package (via NuGet or your preferred method).

Create a LoggingConfig object and configure it.

Initialize Serilog using the library:
```
var loggingConfig = new LoggingConfig
{
    ApplicationName = "MyApp",
    ApplicationVersion = "1.0.0",
    Environment = "Development",
    TimeZone = "UTC",
    EnableGlobalMetadata = true,
    GlobalTags = new List<string> { "api", "v1" },
    OverrideSettings = new LoggingOverrideConfig
    {
        Enabled = true,
        SystemLevel = Serilog.Events.LogEventLevel.Warning,
        MicrosoftLevel = Serilog.Events.LogEventLevel.Warning
    },
    PerLevelSettings = new Dictionary<LogLevelType, LogLevelSettings>
    {
        [LogLevelType.Information] = new LogLevelSettings
        {
            Enabled = true,
            FileLogging = new FileLoggingConfig
            {
                Enabled = true,
                Directory = "logs",
                FileNamePattern = "info_{app}_{Environment}_{time}_.log",
                RollOnFileSizeLimit = true,
                FileSizeLimitBytes = 5 * 1024 * 1024,
                MaxRetainedFiles = 10,
                RollingInterval = "Day",
                EnableCompression = false,
                ArchivePath = "logs/archive",
                OutputTemplate = new OutputTemplateConfig
                {
                    UseDefaultTemplate = true,
                    DefaultFields = new List<string> { "Timestamp", "Level", "Message" }
                }
            },
            SqlLogging = new SqlLoggingConfig
            {
                Enabled = true,
                ConnectionString = "Server=.;Database=LogDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                TableName = "Info_Logs",
                BatchSize = 50,
                EnableFallbackToFile = true
            },
            Enrichers = new EnricherConfig
            {
                Enabled = true,
                EnableCorrelationId = true
            },
            Control = new LogControlConfig
            {
                DisabledTags = new List<string> { "VerboseAPI" },
                MinLevelPerTag = new Dictionary<string, LogLevelType>
                {
                    { "Payment", LogLevelType.Warning },
                    { "Auth", LogLevelType.Error }
                }
            }
        }
    }
};

builder.Services.AddHttpContextAccessor();
Log.Logger = LoggerConfigurator.ConfigureLogger(loggingConfig);
builder.Host.UseSerilog(Log.Logger);
```

With **one package and simple configs**, you get a complete, flexible, and production-ready logging system.

