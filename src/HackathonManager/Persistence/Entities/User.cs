using System;
using FastIDs.TypeId;
using NodaTime;

namespace HackathonManager.Persistence.Entities;

public class User
{
    public const int EmailMinLength = 3;
    public const int EmailMaxLength = 254;
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 100;

    public TypeIdDecoded Id { get; set; }

    public Instant CreatedAt { get; set; }

    public Instant UpdatedAt { get; set; }

    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    public required string PasswordHash { get; set; }

    public uint RowVersion { get; set; }
}
