using System.Collections.Frozen;
using System.Collections.Generic;
using FastIDs.TypeId;
using HackathonManager.Interfaces;
using Humanizer;
using NodaTime;

namespace HackathonManager.Persistence.Entities;

public class User : IRowVersion, IUniqueConstraintMapping
{
    public const int EmailMinLength = 3;
    public const int EmailMaxLength = 254;
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 100;

    /// <inheritdoc />
    public static FrozenDictionary<string, string> UniqueConstraintMappings { get; } =
        new Dictionary<string, string>
        {
            { "users_display_name_key", nameof(DisplayName).Camelize() },
            { "users_email_key", nameof(Email).Camelize() },
        }.ToFrozenDictionary();

    public TypeIdDecoded Id { get; set; }

    public Instant CreatedAt { get; set; }

    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    public required string PasswordHash { get; set; }

    public ICollection<UserAudit> AuditEvents { get; set; } = [];

    public uint RowVersion { get; set; }
}
