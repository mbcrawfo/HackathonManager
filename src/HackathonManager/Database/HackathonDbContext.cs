using Microsoft.EntityFrameworkCore;

namespace HackathonManager.Database;

public class HackathonDbContext : DbContext
{
    public HackathonDbContext(DbContextOptions<HackathonDbContext> options)
        : base(options) { }
}
