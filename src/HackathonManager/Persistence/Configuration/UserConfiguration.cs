using HackathonManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HackathonManager.Persistence.Configuration;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users").HasKey(x => x.Id);

        // *** Columns ***
        builder
            .Property(x => x.Id)
            .IsRequired()
            .HasConversion(ResourceTypes.UserIdValueConverter)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CreatedAt).IsRequired().ValueGeneratedOnAdd();

        builder.Property(x => x.Email).IsRequired().HasMaxLength(User.EmailMaxLength);

        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(User.DisplayNameMaxLength);

        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(1000);

        builder.Property(x => x.RowVersion).IsRowVersion();

        // *** Relationships ***
        builder.HasMany(x => x.AuditEvents).WithOne(x => x.User).HasForeignKey(x => x.UserId);
    }
}
