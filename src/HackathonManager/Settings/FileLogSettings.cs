using FluentValidation;
using Serilog.Events;

namespace HackathonManager.Settings;

public sealed class FileLogSettings : IConfigurationSettings
{
    public bool Enabled { get; init; }

    public LogEventLevel Level { get; init; } = LogEventLevel.Verbose;

    public string Path { get; init; } = "./logs/log.txt";

    /// <inheritdoc />
    public static string ConfigurationSection => "FileLog";
}

public sealed class FileLogSettingsValidator : AbstractValidator<FileLogSettings>
{
    public FileLogSettingsValidator()
    {
        When(
            x => x.Enabled,
            () =>
            {
                RuleFor(x => x.Level).IsInEnum();
                RuleFor(x => x.Path).NotEmpty();
            }
        );
    }
}
