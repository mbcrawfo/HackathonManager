using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Net.Http.Headers;
using Serilog.Events;

namespace HackathonManager.Settings;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class RequestLoggingSettings : IConfigurationSettings
{
    public bool LogBody { get; init; }

    public LogEventLevel BodyLogLevel { get; init; } = LogEventLevel.Verbose;

    public int MaxBodySize { get; init; } = 1024 * 4; // 4 KB

    public string[] ContentTypes { get; init; } = ["*/*"];

    public static string ConfigurationSection => "RequestLogging";
}

[UsedImplicitly]
public sealed class RequestLoggingSettingsValidator : AbstractValidator<RequestLoggingSettings>
{
    public RequestLoggingSettingsValidator()
    {
        When(
            x => x.LogBody,
            () =>
            {
                RuleFor(x => x.BodyLogLevel).IsInEnum();
                RuleFor(x => x.MaxBodySize).GreaterThan(0);
                RuleFor(x => x.ContentTypes).NotEmpty();
                RuleForEach(x => x.ContentTypes)
                    .Must(s => MediaTypeHeaderValue.TryParse(s, out _))
                    .WithMessage("Invalid content type format.");
            }
        );
    }
}
