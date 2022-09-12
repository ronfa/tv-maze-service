using CodingChallenge.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodingChallenge.Infrastructure.Logging;

public class CustomLambdaLoggerConfig
{
    public LogLevel LogLevel { get; set; } = LogLevel.Warning;
    public int EventId { get; set; } = 0;
    public ConsoleColor Color { get; set; } = ConsoleColor.Yellow;
    public IInfrastructureProject? InfrastructureProject { get; set; }
}


