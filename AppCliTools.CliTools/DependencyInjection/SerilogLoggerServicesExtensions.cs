using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace AppCliTools.CliTools.DependencyInjection;

public static class SerilogLoggerServicesExtensions
{
    public static IServiceCollection AddSerilogLoggerService(this IServiceCollection services,
        LogEventLevel consoleLogEventLevel, string appName, string? logFolder = null, string? logFileName = null)
    {
        CreateLogger(consoleLogEventLevel, appName, logFileName, logFolder);
        services.AddLogging(configure => configure.AddSerilog());
        services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);
        return services;
    }

    private static void CreateLogger(LogEventLevel consoleLogEventLevel, string appName, string? logFileName,
        string? logFolder)
    {
        try
        {
            string? logFile = null;
            if (!string.IsNullOrWhiteSpace(logFileName))
            {
                logFile = logFileName;
            }
            else if (logFolder is not null)
            {
                logFile = Path.Combine(logFolder, appName, $"{appName}.log");
            }

            if (logFile is null)
            {
                return;
            }

            const string extension = ".log";
            if (logFile.ToUpperInvariant().EndsWith(".LOG", StringComparison.Ordinal) ||
                logFile.ToUpperInvariant().EndsWith(".TXT", StringComparison.Ordinal))
            {
                logFile = logFile[..^4];
            }

            logFile += extension;
            Log.Logger = new LoggerConfiguration().WriteTo
                .Console(consoleLogEventLevel, formatProvider: CultureInfo.InvariantCulture).WriteTo.File(logFile,
                    encoding: Encoding.UTF8, rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture).CreateLogger();
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
