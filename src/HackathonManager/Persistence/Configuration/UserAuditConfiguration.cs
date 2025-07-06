using System.Text.Json;
using System.Text.Json.Serialization;
using HackathonManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HackathonManager.Persistence.Configuration;

public sealed class UserAuditConfiguration : IEntityTypeConfiguration<UserAudit>
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UserAudit> builder)
    {
        builder.ToTable("user_audit");

        // ***Columns ***
        builder.Property<long>("Id").ValueGeneratedOnAdd();

        builder.Property(x => x.UserId).IsRequired().HasConversion(ResourceTypes.UserIdValueConverter);

        builder.Property(x => x.Timestamp).IsRequired();

        builder
            .Property(x => x.Metadata)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                m => JsonSerializer.Serialize(m, SerializerOptions),
                s => JsonSerializer.Deserialize<UserAuditEventMetadata>(s, SerializerOptions)!
            );

        builder.HasKey("Id");
    }
}
