using System.ComponentModel.DataAnnotations.Schema;
using FastIDs.TypeId;
using Microsoft.EntityFrameworkCore;

namespace HackathonManager.Persistence;

public class HackathonDbContext : DbContext
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; protected set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Test>()
            .Property(x => x.Id)
            .HasConversion(typeId => typeId.Id, id => TypeIdDecoded.FromUuidV7("test", id));

        modelBuilder.Entity<Test>().Property(x => x.Version).IsRowVersion();
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
