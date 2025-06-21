using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Tests.TestInfrastructure.Fakes;

public class FakeLogger : ILogger
{
    private readonly List<string> _messages = [];
    private readonly List<object> _scopeStates = [];

    public LogLevel Level { get; set; } = LogLevel.Trace;

    public IReadOnlyList<object> ScopeStates => _scopeStates.AsReadOnly();

    public IReadOnlyList<string> Messages => _messages.AsReadOnly();

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
        where TState : notnull
    {
        _scopeStates.Add(state);
        return new Scope();
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (IsEnabled(logLevel))
        {
            _messages.Add(formatter(state, exception));
        }
    }

    [SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly")]
    private class Scope : IDisposable
    {
        /// <inheritdoc />
        public void Dispose()
        {
            // Intentionally empty
        }
    }
}

public class FakeLogger<T> : FakeLogger, ILogger<T>;
