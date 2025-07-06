using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using FastIDs.TypeId;
using HackathonManager.Persistence.Entities;
using HackathonManager.Persistence.Enums;
using Microsoft.EntityFrameworkCore;

namespace HackathonManager.Persistence;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class HackathonDbContext : DbContext
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options)
        : base(options)
    {
        SavingChanges += ValidateUserAuditMetadata;
    }

    public DbSet<Test> Tests { get; protected set; }

    public DbSet<User> Users { get; protected set; }

    public DbSet<UserAudit> UserAuditEvents { get; protected set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder
            .Entity<Test>()
            .Property(x => x.Id)
            .HasConversion(typeId => typeId.Id, id => TypeIdDecoded.FromUuidV7("test", id));

        modelBuilder.Entity<Test>().Property(x => x.Version).IsRowVersion();
    }

    private void ValidateUserAuditMetadata(object? sender, SavingChangesEventArgs _)
    {
        var userAuditEntries = ChangeTracker
            .Entries<UserAudit>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity);

        foreach (var ua in userAuditEntries)
        {
            if (!UserAuditEventMetadata.TypeMap.TryGetValue(ua.Event, out var expectedType))
            {
                throw new InvalidOperationException(
                    $"{nameof(UserAuditEventType)}.{ua.Event.ToString()} must be added to {nameof(UserAuditEventMetadata)}.{nameof(UserAuditEventMetadata.TypeMap)}."
                );
            }

            if (ua.Metadata.GetType() != expectedType)
            {
                throw new InvalidOperationException(
                    $"A {nameof(UserAudit)}'s {nameof(UserAudit.Metadata)} type is {ua.Metadata.GetType().Name}, does does not match expecte type {expectedType.Name}."
                );
            }
        }
    }
}

[Table("test")]
public class Test
{
    public TypeIdDecoded Id { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public uint Version { get; set; }
}
