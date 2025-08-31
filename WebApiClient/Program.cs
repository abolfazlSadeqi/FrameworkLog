using FrameworkLog.Classes;
using FrameworkLog.Models;
using FrameworkLog.Models.Base;
using FrameworkLog.Models.Others;
using FrameworkLog.Models.Rotate_Archive;
using FrameworkLog.Models.Sink;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


var loggingConfig = new LoggingConfig
{
    ApplicationName = "MyApp",
    ApplicationVersion = "1.0.0",
    Environment = "Development",
    TimeZone = "UTC",
   
    EnableGlobalMetadata = true,
    OverrideSettings = new LoggingOverrideConfig()
    {
        Enabled = true,
        SystemLevel = Serilog.Events.LogEventLevel.Warning,
        MicrosoftLevel = Serilog.Events.LogEventLevel.Warning
    },

    GlobalTags = new List<string> { "api", "v1" },
    PerLevelSettings = new Dictionary<LogLevelType, LogLevelSettings>
    {
        [LogLevelType.Information] = new LogLevelSettings
        {
            Enabled = true,
            Control = new LogControlConfig
            {
                DisabledTags = new List<string> { "VerboseAPI" }, 
                MinLevelPerTag = new Dictionary<string, LogLevelType>
                    {
                        { "Payment", LogLevelType.Warning },
                        { "Auth", LogLevelType.Error }       
                    }
            },

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
                OutputTemplate = new OutputTemplateConfig() { UseDefaultTemplate = true, DefaultFields = new List<string>() { "Timestamp" } }
            },
            SqlLogging = new SqlLoggingConfig
            {
                Enabled = true,
                ConnectionString = "Server=.;Database=LogDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                TableName = "Info_Logs",
                BatchSize = 50,
                EnableFallbackToFile = true
            },
            Enrichers = new EnricherConfig() { Enabled = true, EnableCorrelationId = true },

        },

        [LogLevelType.Error] = new LogLevelSettings
        {
            Enabled = true,
            FileLogging = new FileLoggingConfig
            {
                Enabled = true,
                Directory = "logs",
                FileNamePattern = "error_.log",
                RollOnFileSizeLimit = true,
                FileSizeLimitBytes = 10 * 1024 * 1024,
                MaxRetainedFiles = 20,
                RollingInterval = "Day",
                EnableCompression = true,
                ArchivePath = "logs/archive",
                OutputTemplate = new OutputTemplateConfig() { UseDefaultTemplate = true, 
                    CustomTemplate = " {RequestHeadersdata:RequestHeaders} {RequestBodydata:RequestBody} {IncludeStackTraces:IncludeStackTrace} {IncludeInnerExceptionsdata: IncludeInnerExceptions} {IncludeContexts:IncludeContext} {AuditEnableddata:AuditEnabled} {Timestamp} {Message}"


                }

            },
            SqlLogging = new SqlLoggingConfig
            {
                Enabled = true,
                ConnectionString = "Server=.;Database=LogDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                TableName = "Error_Logs",
                BatchSize = 50,
                OutputTemplate = new OutputTemplateConfig() { UseDefaultTemplate = true, CustomTemplate = " {Timestamp} {Message}" },
                EnableFallbackToFile = true
            }
        }
    }
};

builder.Services.AddHttpContextAccessor();

Log.Logger = LoggerConfigurator.ConfigureLogger(loggingConfig);


builder.Host.UseSerilog(Log.Logger);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
