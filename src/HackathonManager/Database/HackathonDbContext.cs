using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HackathonManager.Database;

public class HackathonDbContext : DbContext
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; protected set; }
}

[Table("test")]
public class Test
{
    public int Id { get; set; }

    public required string Name { get; set; }
}
