using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using HackathonManager.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Sqids;

namespace HackathonManager.Features;

public sealed class GetTest : EndpointWithoutRequest<IReadOnlyCollection<TestDto>>
{
    private readonly HackathonDbContext _dbContext;
    private readonly SqidsEncoder<uint> _versionEncoder;
    private readonly IClock _clock;

    public GetTest(HackathonDbContext dbContext, SqidsEncoder<uint> versionEncoder, IClock clock)
    {
        _dbContext = dbContext;
        _versionEncoder = versionEncoder;
        _clock = clock;
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
        Response = data.ConvertAll(x => new TestDto(
            x.Id.Encode(),
            x.Name,
            _versionEncoder.Encode(x.Version),
            _clock.GetCurrentInstant()
        ));
    }
}
