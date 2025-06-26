using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using HackathonManager.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace HackathonManager.Features;

public sealed class GetTest : EndpointWithoutRequest<IReadOnlyCollection<TestDto>>
{
    private readonly HackathonDbContext _dbContext;

    public GetTest(HackathonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Get("/test");
        AllowAnonymous();
        Options(x => x.WithVersionSet("Test").MapToApiVersion(1.0));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var data = await _dbContext.Tests.ToListAsync(ct);
        Response = data.ConvertAll(x => new TestDto(x.Id.Encode(), x.Name));
    }
}
