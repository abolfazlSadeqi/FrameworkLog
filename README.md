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

---

# **FrameworkLog – Logging Configuration Classes Explained**

---

## **1️⃣ LogLevelType (Enum)**

```csharp
public enum LogLevelType
{
    Trace, Debug, Information, Warning, Error, Fatal
}
```

* Represents different **log levels**.
* Use these levels to configure logging differently for each severity.
* Examples:

  * `Trace` – very detailed logs for debugging.
  * `Information` – general application events.
  * `Error` / `Fatal` – critical issues that need attention.

---

## **2️⃣ LoggingConfig (Main Configuration Class)**

```csharp
public class LoggingConfig
{
    public string ApplicationName { get; set; }
    public string ApplicationVersion { get; set; }
    public string Environment { get; set; } = "Development";
    public string TimeZone { get; set; } = "UTC";
    public bool EnableGlobalMetadata { get; set; } = true;
    public List<string> GlobalTags { get; set; } = new();
    public LoggingOverrideConfig OverrideSettings { get; set; } = new();
    public Dictionary<LogLevelType, LogLevelSettings> PerLevelSettings { get; set; } = new();
}
```

### **Purpose:**

* Central configuration object for **your logging setup**.
* Contains **application-wide metadata**, global tags, and per-level settings.

### **Key Properties:**

* `ApplicationName` / `ApplicationVersion` – automatically added to logs.
* `Environment` – e.g., Development, Staging, Production.
* `TimeZone` – add timezone information in log metadata.
* `EnableGlobalMetadata` – include global tags and properties in all logs.
* `GlobalTags` – tags like `api`, `v1` for filtering or grouping logs.
* `PerLevelSettings` – define detailed logging behavior **per log level**.

---

## **3️⃣ LoggingOverrideConfig**

```csharp
public class LoggingOverrideConfig
{
    public bool Enabled { get; set; } = false;
    public LogEventLevel MicrosoftLevel { get; set; } = LogEventLevel.Warning;
    public LogEventLevel SystemLevel { get; set; } = LogEventLevel.Warning;
}
```

### **Purpose:**

* Override log levels for **specific namespaces** like Microsoft or System.
* Prevents noisy logs from external libraries.

---

## **4️⃣ LogLevelSettings**

```csharp
public class LogLevelSettings
{
    public bool Enabled { get; set; } = true;
    public FileLoggingConfig FileLogging { get; set; }
    public SqlLoggingConfig SqlLogging { get; set; }
    public ElkLoggingConfig ElkLogging { get; set; }
    public EnricherConfig Enrichers { get; set; }
    public LogControlConfig Control { get; set; }
    public PerformanceConfig Performance { get; set; }
    public ArchivingConfig Archiving { get; set; }
    public LogRotateConfig LogRotate { get; set; }
}
```

### **Purpose:**

* Configure **logging behavior per log level**.
* Supports **multiple sinks**, enrichers, performance tuning, retention, and rotation.

---

## **5️⃣ EnricherConfig**

```csharp
public class EnricherConfig
{
    public bool Enabled { get; set; }
    public bool EnableCorrelationId { get; set; }
    public List<string> ActiveEnrichers { get; set; } = new();
}
```

### **Purpose:**

* Add extra information to logs automatically.
* `EnableCorrelationId` – trace requests across services.
* `ActiveEnrichers` – e.g., `MachineName`, `ThreadId`.

---

## **6️⃣ FileLoggingConfig**

```csharp
public class FileLoggingConfig
{
    public bool Enabled { get; set; }
    public string Directory { get; set; }
    public string FileNamePattern { get; set; } = "log_{level}_{yyyyMMdd}.log";
    public string RollingInterval { get; set; } = "Day";
    public bool RollOnFileSizeLimit { get; set; }
    public long? FileSizeLimitBytes { get; set; }
    public int? MaxRetainedFiles { get; set; }
    public bool EnableCompression { get; set; }
    public string ArchivePath { get; set; }
    public OutputTemplateConfig OutputTemplate { get; set; }
}
```

### **Purpose:**

* Configure logging to files.
* Features:

  * **Rolling files** by date or size.
  * **Max retained files** to avoid unlimited growth.
  * **Compression and archiving** for old logs.
  * **Custom file naming** per log level.
  * **Output template** for custom log format.

---

## **7️⃣ SqlLoggingConfig**

```csharp
public class SqlLoggingConfig
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; }
    public string TableName { get; set; } = "Logs";
    public int BatchSize { get; set; } = 50;
    public bool EnableFallbackToFile { get; set; }
    public OutputTemplateConfig OutputTemplate { get; set; }
}
```

### **Purpose:**

* Log events into **SQL databases**.
* Supports batching and fallback to file if SQL fails.
* Allows **custom column selection** via `OutputTemplate`.

---

## **8️⃣ ElkLoggingConfig**

```csharp
public class ElkLoggingConfig
{
    public bool Enabled { get; set; }
    public string Url { get; set; }
    public string IndexPattern { get; set; } = "logs-{level}-{yyyy.MM.dd}";
    public bool AutoRegisterTemplate { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public OutputTemplateConfig OutputTemplate { get; set; }
}
```

### **Purpose:**

* Send logs to **Elasticsearch / ELK stack**.
* Customize index per log level.
* Supports batching and structured output.

---

## **9️⃣ OutputTemplateConfig**

```csharp
public class OutputTemplateConfig
{
    public bool UseDefaultTemplate { get; set; } = true;
    public string? CustomTemplate { get; set; }
    public List<string> DefaultFields { get; set; } = new();
}
```

### **Purpose:**

* Control **log message format**.
* Use default templates or custom patterns.
* Include fields like `Timestamp`, `Level`, `Message`, `CorrelationId`, etc.

---

## **🔹 Additional Supporting Configs**

### **PerformanceConfig**

```csharp
public class PerformanceConfig
{
    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 50;
    public int FlushIntervalMs { get; set; } = 2000;
    public bool AsyncLogging { get; set; } = true;
}
```

* Enables **async logging**, batching, and flush intervals.

### **LogControlConfig**

```csharp
public class LogControlConfig
{
    public List<string> DisabledTags { get; set; } = new();
    public Dictionary<string, LogLevelType> MinLevelPerTag { get; set; } = new();
}
```

* Control logging **per tag**, disable certain tags or enforce minimum levels.

### **ArchivingConfig**

```csharp
public class ArchivingConfig
{
    public bool Enabled { get; set; }
    public string ArchivePath { get; set; }
    public int RetentionDays { get; set; } = 30;
}
```

* Automatically move old logs to archive folder and delete after retention period.

### **LogRotateConfig**

```csharp
public class LogRotateConfig
{
    public bool Enabled { get; set; }
    public string RotationFrequency { get; set; } = "Day";
    public int MaxFileSizeMB { get; set; } = 100;
    public int MaxRetainedFiles { get; set; } = 10;
}
```

* Rotate log files automatically to avoid huge single files.


Example 1 :

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
                FileSizeLimitBytes = 5 * 1024 * 1024, // 5 MB
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

```

// Initialize logger
```
builder.Services.AddHttpContextAccessor();
Log.Logger = LoggerConfigurator.ConfigureLogger(loggingConfig);
builder.Host.UseSerilog(Log.Logger);
```
✅ Explanation
File logging only: Logs are written to a specific directory (logs) with a custom filename pattern.

Rolling & retention: Automatically rolls daily and keeps the last 10 files.

Tags & control: You can disable logs for specific tags or enforce minimum levels per tag.

Correlation ID: Included automatically for tracing requests.

Global metadata: AppName, Version, Environment, TimeZone, and global tags are included in every log.


