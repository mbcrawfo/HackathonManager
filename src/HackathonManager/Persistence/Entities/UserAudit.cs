using System;
using System.Diagnostics.CodeAnalysis;
using FastIDs.TypeId;
using HackathonManager.Persistence.Enums;
using NodaTime;

namespace HackathonManager.Persistence.Entities;

public class UserAudit
{
    public TypeIdDecoded UserId { get; set; }

    public UserAuditEventType Event { get; set; }

    public Instant Timestamp { get; set; }

    public UserAuditEventMetadata Metadata { get; set; } = new();

    public User? User { get; set; }

    public T GetMetadata<T>()
        where T : UserAuditEventMetadata
    {
        return Metadata as T
            ?? throw new InvalidOperationException(
                $"Expected {nameof(Metadata)} to be of type {typeof(T).Name}, but it was type {Metadata.GetType().Name}."
            );
    }

    public bool TryGetMetadata<T>([NotNullWhen(true)] out T? metadata)
        where T : UserAuditEventMetadata
    {
        if (Metadata is not T m)
        {
            metadata = null;
            return false;
        }

        metadata = m;
        return true;
    }
}
