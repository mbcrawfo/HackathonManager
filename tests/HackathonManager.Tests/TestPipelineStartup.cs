using System.Threading.Tasks;
using Xunit.Sdk;
using Xunit.v3;

namespace HackathonManager.Tests;

public sealed class TestPipelineStartup : ITestPipelineStartup
{
    /// <inheritdoc />
    public ValueTask StartAsync(IMessageSink diagnosticMessageSink) => ValueTask.CompletedTask;

    /// <inheritdoc />
    public ValueTask StopAsync() => ValueTask.CompletedTask;
}
