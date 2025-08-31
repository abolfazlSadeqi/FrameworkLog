using FrameworkLog.Models.Base;
using FrameworkLog.Models.Others;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Data;
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

        loggerConfig.Enrich.FromLogContext();

        if (config.OverrideSettings.Enabled)
        {
            loggerConfig.MinimumLevel.Override("Microsoft", config.OverrideSettings.MicrosoftLevel);
            loggerConfig.MinimumLevel.Override("System", config.OverrideSettings.SystemLevel);
        }


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
                    }
                }
            }
            if (settings.Enrichers?.Enabled == true && settings.Enrichers.EnableCorrelationId)
            {
                var httpContextAccessor = new HttpContextAccessor();
                loggerConfig.Enrich.WithCorrelationId();
                loggerConfig.Enrich.WithCorrelationIdHeader();

            }

            // 📌 File Logging
            if (settings.FileLogging?.Enabled == true)
            {
                var logFilePath = Path.Combine(settings.FileLogging.Directory,
                    settings.FileLogging.FileNamePattern
                    .Replace("{level}", level.ToString())
                    .Replace("{date}", DateTime.Now.ToString("yyyyMMdd"))
                    .Replace("{time}", DateTime.Now.ToString("HHmmss"))
                    .Replace("{app}", config.ApplicationName)
                    .Replace("{Environment}", config.Environment)

                    );


                loggerConfig.WriteTo.Logger(lc => lc
                   .Filter.ByIncludingOnly(le => le.Level == serilogLevel)
                   .WriteTo.File(
                       path: logFilePath,
                       rollingInterval: Enum.TryParse<RollingInterval>(
                           settings.FileLogging.RollingInterval, true, out var interval)
                           ? interval : RollingInterval.Day,
                       rollOnFileSizeLimit: settings.FileLogging.RollOnFileSizeLimit,
                       fileSizeLimitBytes: settings.FileLogging.FileSizeLimitBytes,
                       retainedFileCountLimit: settings.FileLogging.MaxRetainedFiles,
                       outputTemplate: BuildOutputTemplate(config,settings, settings.FileLogging.OutputTemplate)
                   )
               );


                
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

                    var columnOptions = new Serilog.Sinks.MSSqlServer.ColumnOptions();
                    if (!string.IsNullOrWhiteSpace(settings.SqlLogging.OutputTemplate?.CustomTemplate))
                    {

                        columnOptions.Store.Remove(StandardColumn.Properties);
                        columnOptions.AdditionalColumns = settings.SqlLogging.OutputTemplate.DefaultFields
                            .Select(f => new SqlColumn { ColumnName = f, DataType = SqlDbType.NVarChar })
                            .ToList();
                    }

                    loggerConfig.WriteTo.MSSqlServer(
                        connectionString: settings.SqlLogging.ConnectionString,
                        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                        {
                            TableName = settings.SqlLogging.TableName,
                            AutoCreateSqlTable = true
                        },
                        columnOptions: columnOptions,
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
                    MinimumLogEventLevel = serilogLevel,
                    CustomFormatter = new CustomElkFormatter(settings.ElkLogging.OutputTemplate)
                });
            }

            // 📌 Request Logging
            if (settings.RequestLogging?.Enabled == true)
            {

                var enricher = new HttpRequestEnricher(settings.RequestLogging);
                loggerConfig.Enrich.With(enricher);

             
            }

            // 📌 Exception Logging
            if (settings.ExceptionLogging?.Enabled == true)
            {
                loggerConfig.Enrich.WithProperty("IncludeStackTrace", settings.ExceptionLogging.IncludeStackTrace);
                loggerConfig.Enrich.WithProperty("IncludeInnerExceptions", settings.ExceptionLogging.IncludeInnerExceptions);
            }

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

    private static string BuildOutputTemplate(LoggingConfig config, LogLevelSettings settings, OutputTemplateConfig? cfg)
    {
        string CustomTemplate = "";
        if (config.EnableGlobalMetadata && config.GlobalTags != null && config.GlobalTags.Any())
            CustomTemplate = "[Tags:{GlobalTags}]";

        if (settings.Enrichers?.Enabled == true && settings.Enrichers.EnableCorrelationId)
            CustomTemplate = CustomTemplate+ " [CorrelationId:{CorrelationId}] ";

        if (cfg == null)
            return CustomTemplate + "{Message:lj}{NewLine}";

        if (!string.IsNullOrWhiteSpace(cfg.CustomTemplate))
            return CustomTemplate + cfg.CustomTemplate;

        if (cfg.UseDefaultTemplate && cfg.DefaultFields.Any())
        {
            var parts = cfg.DefaultFields.Select(f => f switch
            {
                "Timestamp" => "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}",
                "Level" => "[{Level:u3}]",
                "AppName" => "[{AppName}]",
                "Environment" => "[{Environment}]",
                "ThreadId" => "[Thread:{ThreadId}]",
                "Exception" => "{Exception}",
                _ => $"{{{f}}}"
            });


            return CustomTemplate + string.Join(" ", parts) + "{Message:lj}" +"{NewLine}";
        }

        return CustomTemplate + "{Message:lj}{NewLine}";
    }

}
