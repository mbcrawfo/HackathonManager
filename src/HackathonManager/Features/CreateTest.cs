using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastIDs.TypeId;
using FluentValidation;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using Microsoft.AspNetCore.Builder;
using NodaTime;
using Sqids;

namespace HackathonManager.Features;

public sealed class CreateTestRequest
{
    /// <summary>
    ///     Optional id.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    ///     Name of the test.
    /// </summary>
    public required string Name { get; init; }
}

public sealed class CreateTestValidator : Validator<CreateTestRequest>
{
    public CreateTestValidator()
    {
        RuleFor(x => x.Id).MustBeValidTypeIdWithCode("test").When(x => x.Id is not null);
        RuleFor(x => x.Name).NotNullOrEmptyWithCode().MaxLengthWithCode(100);
    }
}

public class CreateTest : Endpoint<CreateTestRequest, TestDto>
{
    private readonly IClock _clock;
    private readonly HackathonDbContext _dbContext;
    private readonly SqidsEncoder<uint> _versionEncoder;

    public CreateTest(HackathonDbContext dbContext, SqidsEncoder<uint> versionEncoder, IClock clock)
    {
        _dbContext = dbContext;
        _versionEncoder = versionEncoder;
        _clock = clock;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Post("/test");
        AllowAnonymous();
        Options(x => x.WithVersionSet("Test").MapToApiVersion(1.0));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateTestRequest req, CancellationToken ct)
    {
        var id = req.Id is null ? TypeIdDecoded.New("test") : TypeId.Parse(req.Id).Decode();
        var test = new Test { Id = id, Name = req.Name };
        _dbContext.Tests.Add(test);
        await _dbContext.SaveChangesAsync(ct);
        Response = new TestDto(
            test.Id.Encode(),
            test.Name,
            _versionEncoder.Encode(test.Version),
            _clock.GetCurrentInstant()
        );
    }
}
