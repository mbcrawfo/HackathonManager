using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Tests.TestInfrastructure.Fakes;

[SuppressMessage("Major Code Smell", "S3881:\"IDisposable\" should be implemented correctly")]
public class FakeLoggerFactory : ILoggerFactory
{
    private readonly ILogger _logger;

    public FakeLoggerFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Intentionally empty
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => _logger;

    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider)
    {
        // Intentionally empty
    }
}
