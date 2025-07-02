using HackathonManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HackathonManager.Persistence.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired().HasConversion(ResourceTypes.UserIdValueConverter);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(User.EmailMaxLength);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(User.DisplayNameMaxLength);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
