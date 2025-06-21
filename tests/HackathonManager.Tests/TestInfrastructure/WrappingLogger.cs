using System;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Tests.TestInfrastructure;

public class WrappingLogger<T> : ILogger<T>
{
    private readonly ILogger _logger;

    public WrappingLogger(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => _logger.BeginScope(state);
}
