using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using HackathonManager.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HackathonManager.Features;

public class Hello : EndpointWithoutRequest
{
    private readonly HackathonDbContext _dbContext;

    public Hello(HackathonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Get("/hello");
        AllowAnonymous();
        Options(x => x.WithVersionSet("Test").MapToApiVersion(1.0));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        Logger.LogInformation("Hello World");
        Response = await _dbContext.Tests.ToListAsync(ct);
    }
}
