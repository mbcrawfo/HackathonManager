using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HackathonManager.Persistence.Enums;

namespace HackathonManager.Persistence.Entities;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(UserRegistrationMetadataV1), "UserRegistrationMetadataV1")]
[JsonDerivedType(typeof(UserEmailChangedMetadataV1), "UserEmailChangedMetadataV1")]
[JsonDerivedType(typeof(UserDisplayNameChangedMetadataV1), "UserDisplayNameChangedMetadataV1")]
public record UserAuditEventMetadata
{
    public static readonly FrozenDictionary<UserAuditEventType, Type> TypeMap = new Dictionary<UserAuditEventType, Type>
    {
        { UserAuditEventType.Registration, typeof(UserRegistrationMetadataV1) },
        { UserAuditEventType.EmailChanged, typeof(UserEmailChangedMetadataV1) },
        { UserAuditEventType.DisplayNameChanged, typeof(UserDisplayNameChangedMetadataV1) },
    }.ToFrozenDictionary();
}

public sealed record UserRegistrationMetadataV1(string Email, string DisplayName) : UserAuditEventMetadata;

public sealed record UserEmailChangedMetadataV1(string NewEmail, string? OldEmail) : UserAuditEventMetadata;

public sealed record UserDisplayNameChangedMetadataV1(string NewDisplayName, string? OldDisplayName)
    : UserAuditEventMetadata;
