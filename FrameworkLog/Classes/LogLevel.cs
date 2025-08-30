using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Collections.Generic;


namespace FrameworkLog.Classes;

// سطح‌بندی لاگ
public enum LogLevelType
{
    Trace, Debug, Information, Warning, Error, Fatal
}



