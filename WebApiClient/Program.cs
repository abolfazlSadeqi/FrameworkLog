using FrameworkLog.Classes;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ------------------ تنظیمات لاگ --------------------
var loggingConfig = new LoggingConfig
{
    ApplicationName = "MyApp",
    ApplicationVersion = "1.0.0",
    Environment = "Development",
    TimeZone = "UTC",
    EnableGlobalMetadata = true,
    GlobalTags = new List<string> { "api", "v1" },
    PerLevelSettings = new Dictionary<LogLevelType, LogLevelSettings>
    {
        [LogLevelType.Information] = new LogLevelSettings
        {
            Enabled = true,
            FileLogging = new FileLoggingConfig
            {
                Enabled = true,
                Directory = "logs",
                FileNamePattern = "info_{level}.log",
                RollOnFileSizeLimit = true,
                FileSizeLimitBytes = 5 * 1024 * 1024,
                MaxRetainedFiles = 10,
                RollingInterval = "Day",
                EnableCompression = false,
                ArchivePath = "logs/archive"
            },
            SqlLogging = new SqlLoggingConfig
            {
                Enabled = true,
                ConnectionString = "Server=.;Database=LogDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                TableName = "Logs",
                BatchSize = 50,
                EnableFallbackToFile = true
            }
        },

        [LogLevelType.Error] = new LogLevelSettings
        {
            Enabled = true,
            FileLogging = new FileLoggingConfig
            {
                Enabled = true,
                Directory = "logs",
                FileNamePattern = "error_{level}.log",
                RollOnFileSizeLimit = true,
                FileSizeLimitBytes = 10 * 1024 * 1024,
                MaxRetainedFiles = 20,
                RollingInterval = "Day",
                EnableCompression = true,
                ArchivePath = "logs/archive"
            },
            SqlLogging = new SqlLoggingConfig
            {
                Enabled = true,
                ConnectionString = "Server=.;Database=LogDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                TableName = "Logs",
                BatchSize = 50,
                EnableFallbackToFile = true
            }
        }
    }
};

// این خط خیلی مهمه 👇
Log.Logger = LoggerConfigurator.ConfigureLogger(loggingConfig);

// حالا ASP.NET Core بدونه از Serilog استفاده کنه 👇
builder.Host.UseSerilog(Log.Logger);

// ------------------ Service ها --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------ Middleware --------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
