using System.Threading.Tasks;
using HackathonManager.Services;
using Xunit.Sdk;
using Xunit.v3;

namespace HackathonManager.Tests;

public sealed class TestPipelineStartup : ITestPipelineStartup
{
    /// <inheritdoc />
    public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
    {
        // Reduced hashing significantly speeds up integration tests.
        PasswordService.WorkFactor = 4;

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask StopAsync() => ValueTask.CompletedTask;
}
