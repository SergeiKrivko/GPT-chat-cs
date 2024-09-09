using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Utils;

public class LogService
{
    private static ILoggerFactory _factory = LoggerFactory.Create(builder => builder
            .AddFilter("Sockets", LogLevel.Debug)
            .AddFilter("Chats", LogLevel.Debug)
            .AddFilter("Auth", LogLevel.Debug)
            .AddConsole()
        );

    public static ILogger CreateLogger(string category)
    {
        return _factory.CreateLogger(category);
    }
}