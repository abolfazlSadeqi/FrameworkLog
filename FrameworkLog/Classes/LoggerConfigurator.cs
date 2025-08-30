using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FrameworkLog.Classes;

public static class LoggerConfigurator
{
    public static ILogger ConfigureLogger(LoggingConfig config)
    {
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("AppName", config.ApplicationName)
            .Enrich.WithProperty("AppVersion", config.ApplicationVersion)
            .Enrich.WithProperty("Environment", config.Environment);

        // 📌 TimeZone
        if (!string.IsNullOrEmpty(config.TimeZone))
            loggerConfig.Enrich.WithProperty("TimeZone", config.TimeZone);

        // 📌 Global Metadata
        if (config.EnableGlobalMetadata)
            loggerConfig.Enrich.WithProperty("GlobalTags", string.Join(",", config.GlobalTags));

        foreach (var levelConfig in config.PerLevelSettings)
        {
            var level = levelConfig.Key;
            var settings = levelConfig.Value;
            if (!settings.Enabled) continue;

            var serilogLevel = ConvertLogLevel(level);

            // 📌 Enrichers
            if (settings.Enrichers?.Enabled == true && settings.Enrichers.ActiveEnrichers.Any())
            {
                foreach (var enricherName in settings.Enrichers.ActiveEnrichers)
                {
                    switch (enricherName)
                    {
                        case "MachineName":
                            loggerConfig.Enrich.WithMachineName();
                            break;
                        case "ThreadId":
                            loggerConfig.Enrich.WithThreadId();
                            break;
                        case "CorrelationId":
                            loggerConfig.Enrich.WithCorrelationId();
                            break;
                    }
                }
            }

            // 📌 File Logging
            if (settings.FileLogging?.Enabled == true)
            {
                var logFilePath = Path.Combine(settings.FileLogging.Directory,
                    settings.FileLogging.FileNamePattern.Replace("{level}", level.ToString()));

                loggerConfig.WriteTo.File(
                    path: logFilePath,
                    rollingInterval: Enum.TryParse<RollingInterval>(
                        settings.FileLogging.RollingInterval, true, out var interval)
                        ? interval : RollingInterval.Day,
                    rollOnFileSizeLimit: settings.FileLogging.RollOnFileSizeLimit,
                    fileSizeLimitBytes: settings.FileLogging.FileSizeLimitBytes,
                    retainedFileCountLimit: settings.FileLogging.MaxRetainedFiles,
                    restrictedToMinimumLevel: serilogLevel
                   );

                // آرشیو و فشرده‌سازی پس از رول (manually)
                if (settings.FileLogging.EnableCompression && settings.Archiving?.Enabled == true)
                {
                    Task.Run(() => LogFileArchiver.ArchiveOldFiles(
                        settings.FileLogging.Directory,
                        settings.FileLogging.ArchivePath,
                        settings.Archiving.RetentionDays));
                }


                if (settings.Archiving?.Enabled == true)
                {
                    Task.Run(() => ApplyArchiving(settings.FileLogging.Directory,
                        settings.Archiving.ArchivePath, settings.Archiving.RetentionDays));
                }
            }

            // 📌 SQL Logging
            if (settings.SqlLogging?.Enabled == true)
            {
                try
                {
                    loggerConfig.WriteTo.MSSqlServer(
                        connectionString: settings.SqlLogging.ConnectionString,
                        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                        {
                            TableName = settings.SqlLogging.TableName,
                            AutoCreateSqlTable = true,
                            BatchPostingLimit = settings.SqlLogging.BatchSize
                        },
                        restrictedToMinimumLevel: serilogLevel
                    );
                }
                catch
                {
                    if (settings.SqlLogging.EnableFallbackToFile)
                    {
                        loggerConfig.WriteTo.File(
                            $"logs/sql_fallback_{DateTime.UtcNow:yyyyMMdd}.log",
                            restrictedToMinimumLevel: serilogLevel);
                    }
                }
            }

            // 📌 ELK Logging
            if (settings.ElkLogging?.Enabled == true)
            {
                loggerConfig.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(settings.ElkLogging.Url))
                {
                    IndexFormat = settings.ElkLogging.IndexPattern.Replace("{level}", level.ToString().ToLower()),
                    AutoRegisterTemplate = settings.ElkLogging.AutoRegisterTemplate,
                    BatchPostingLimit = settings.ElkLogging.BatchSize,
                    MinimumLogEventLevel = serilogLevel
                });
            }

            // 📌 Request Logging
            if (settings.RequestLogging?.Enabled == true)
            {

                var enricher = new HttpRequestEnricher(settings.RequestLogging);
                loggerConfig.Enrich.With(enricher);  // <-- همین حالت درست است

                if (settings.RequestLogging.MaskSensitiveData)
                {
                    loggerConfig.Enrich.With(new MaskingEnricher(new MaskingConfig
                    {
                        Enabled = true,
                        SensitiveKeys = new() { "Authorization", "Password" }
                    }));
                }
            }

            // 📌 Exception Logging
            if (settings.ExceptionLogging?.Enabled == true)
            {
                loggerConfig.Enrich.WithProperty("IncludeStackTrace", settings.ExceptionLogging.IncludeStackTrace);
                loggerConfig.Enrich.WithProperty("IncludeInnerExceptions", settings.ExceptionLogging.IncludeInnerExceptions);
            }

            // 📌 Masking
            if (settings.Masking?.Enabled == true)
                loggerConfig.Enrich.With(new MaskingEnricher(settings.Masking));

            // 📌 Detailed Logging
            if (settings.DetailedLogging?.Enabled == true)
            {
                loggerConfig.Enrich.WithProperty("IncludeContext", settings.DetailedLogging.IncludeContext);

                if (settings.DetailedLogging.EnableAudit)
                    loggerConfig.Enrich.WithProperty("AuditEnabled", true);
            }

            // 📌 Log Control
            if (settings.Control != null)
            {
                foreach (var disabledTag in settings.Control.DisabledTags)
                {
                    loggerConfig.Filter.ByExcluding(logEvent =>
                        logEvent.Properties.ContainsKey("Tag") &&
                        logEvent.Properties["Tag"].ToString().Trim('"') == disabledTag);
                }

                foreach (var kvp in settings.Control.MinLevelPerTag)
                {
                    var tag = kvp.Key;
                    var minLevel = ConvertLogLevel(kvp.Value);

                    loggerConfig.Filter.ByIncludingOnly(logEvent =>
                        logEvent.Properties.ContainsKey("Tag") &&
                        logEvent.Properties["Tag"].ToString().Trim('"') == tag &&
                        logEvent.Level >= minLevel);
                }
            }

            // 📌 Performance (Async Logging)
            if (settings.Performance?.AsyncLogging == true)
            {
                loggerConfig.WriteTo.Async(sinks =>
                {
                    if (settings.FileLogging?.Enabled == true)
                    {
                        sinks.File($"{settings.FileLogging.Directory}/async_{level}.log",
                            restrictedToMinimumLevel: serilogLevel);
                    }
                    if (settings.SqlLogging?.Enabled == true)
                    {
                        sinks.MSSqlServer(
                            connectionString: settings.SqlLogging.ConnectionString,
                            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                            {
                                TableName = settings.SqlLogging.TableName,
                                AutoCreateSqlTable = true,
                                BatchPostingLimit = settings.SqlLogging.BatchSize
                            },
                            restrictedToMinimumLevel: serilogLevel
                        );
                    }
                    if (settings.ElkLogging?.Enabled == true)
                    {
                        sinks.Elasticsearch(new ElasticsearchSinkOptions(new Uri(settings.ElkLogging.Url))
                        {
                            IndexFormat = settings.ElkLogging.IndexPattern.Replace("{level}", level.ToString().ToLower()),
                            AutoRegisterTemplate = settings.ElkLogging.AutoRegisterTemplate,
                            BatchPostingLimit = settings.ElkLogging.BatchSize,
                            MinimumLogEventLevel = serilogLevel
                        });
                    }
                },
                bufferSize: settings.Performance.BatchSize,
                blockWhenFull: true);
            }

            // 📌 LogRotate
            if (settings.LogRotate?.Enabled == true)
            {
                loggerConfig.WriteTo.File(
                    path: $"logs/rotate_{level}.log",
                    rollingInterval: Enum.TryParse<RollingInterval>(settings.LogRotate.RotationFrequency, out var ri2) ? ri2 : RollingInterval.Day,
                    fileSizeLimitBytes: settings.LogRotate.MaxFileSizeMB * 1024 * 1024,
                    retainedFileCountLimit: settings.LogRotate.MaxRetainedFiles,
                    restrictedToMinimumLevel: serilogLevel
                );
            }
        }

        return loggerConfig.CreateLogger();
    }

    private static LogEventLevel ConvertLogLevel(LogLevelType level) =>
        level switch
        {
            LogLevelType.Trace => LogEventLevel.Verbose,
            LogLevelType.Debug => LogEventLevel.Debug,
            LogLevelType.Information => LogEventLevel.Information,
            LogLevelType.Warning => LogEventLevel.Warning,
            LogLevelType.Error => LogEventLevel.Error,
            LogLevelType.Fatal => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };

    private static void ApplyArchiving(string sourceDir, string archiveDir, int retentionDays)
    {
        if (!Directory.Exists(sourceDir)) return;
        Directory.CreateDirectory(archiveDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*.log"))
        {
            var dest = Path.Combine(archiveDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        foreach (var oldFile in Directory.GetFiles(archiveDir)
                     .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-retentionDays)))
        {
            File.Delete(oldFile);
        }
    }
}

// 📌 Enricher برای ماسک کردن داده‌های حساس
public class MaskingEnricher : ILogEventEnricher
{
    private readonly MaskingConfig _config;
    public MaskingEnricher(MaskingConfig config) => _config = config;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (_config?.SensitiveKeys == null || !_config.SensitiveKeys.Any()) return;

        foreach (var key in _config.SensitiveKeys)
        {
            var prop = logEvent.Properties
                .FirstOrDefault(p => p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (!prop.Equals(default(KeyValuePair<string, LogEventPropertyValue>)))
            {
                var masked = propertyFactory.CreateProperty(key, _config.MaskReplacement);
                logEvent.AddOrUpdateProperty(masked);
            }
        }
    }
}

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
