using FluentValidation;
using Serilog.Events;

namespace HackathonManager.Settings;

public sealed class ConsoleLogSettings : IConfigurationSettings
{
    public ConsoleLogType Type { get; init; } = ConsoleLogType.Disabled;

    public LogEventLevel Level { get; init; } = LogEventLevel.Verbose;

    public string TextTemplate { get; init; } =
        "[{Timestamp:HH:mm:ss} {Level:u3}] {RequestId} {Message:lj}{NewLine}{Exception}";

    /// <inheritdoc />
    public static string ConfigurationSection => "ConsoleLog";
}

public enum ConsoleLogType
{
    Disabled = 0,
    Text = 1,
    Json = 2,
}

public sealed class ConsoleLogSettingsValidator : AbstractValidator<ConsoleLogSettings>
{
    public ConsoleLogSettingsValidator()
    {
        When(
            x => x.Type is not ConsoleLogType.Disabled,
            () =>
            {
                RuleFor(x => x.Type).IsInEnum();

                RuleFor(x => x.TextTemplate).NotEmpty().When(x => x.Type == ConsoleLogType.Text);
            }
        );
    }
}
