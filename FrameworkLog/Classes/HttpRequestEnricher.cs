using FrameworkLog.Models.Others;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace FrameworkLog.Classes;

// 📌 Enricher برای Http Request (Headers + Body)
public class HttpRequestEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        // Headers
        foreach (var h in context.Request.Headers)
        {
            var prop = propertyFactory.CreateProperty($"Header_{h.Key}", h.Value.ToString());
            logEvent.AddPropertyIfAbsent(prop);
        }

        // Body (فقط برای درخواست POST/PUT)
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = reader.ReadToEnd();
            context.Request.Body.Position = 0;

            var prop = propertyFactory.CreateProperty("RequestBody", body);
            logEvent.AddPropertyIfAbsent(prop);
        }

        // مسیر درخواست
        var pathProp = propertyFactory.CreateProperty("RequestPath", context.Request.Path);
        logEvent.AddPropertyIfAbsent(pathProp);
    }
}


public class ExceptionEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Exception == null) return;

        var stackProp = propertyFactory.CreateProperty("StackTrace", logEvent.Exception.StackTrace);
        logEvent.AddPropertyIfAbsent(stackProp);

        if (logEvent.Exception.InnerException != null)
        {
            var innerProp = propertyFactory.CreateProperty("InnerException", logEvent.Exception.InnerException.Message);
            logEvent.AddPropertyIfAbsent(innerProp);
        }
    }
}
