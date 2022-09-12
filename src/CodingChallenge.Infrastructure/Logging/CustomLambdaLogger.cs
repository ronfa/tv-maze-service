using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CodingChallenge.Infrastructure.Logging;

public record LogEntry(string Message, string Environment, string Platform, string System, string Subsystem,
    string Version, LogLevel LogLevel, int LogEventId, string LogName);

public class CustomLambdaLogger : ILogger
{
    private readonly CustomLambdaLoggerConfig _config;
    private readonly string _name;

    public CustomLambdaLogger(string name, CustomLambdaLoggerConfig config)
    {
        _name = name;
        _config = config;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= _config.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        LambdaLogger.Log(
            JsonConvert.SerializeObject(
                new LogEntry(formatter(state, exception),
                    _config.InfrastructureProject!.Environment,
                    _config.InfrastructureProject.Platform,
                    _config.InfrastructureProject.System,
                    _config.InfrastructureProject.Subsystem,
                    _config.InfrastructureProject.Version, logLevel, eventId.Id, _name)));
    }
}