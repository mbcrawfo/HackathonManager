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
    public TypeId? Id { get; init; }

    /// <summary>
    ///     Name of the test.
    /// </summary>
    public required string Name { get; init; }

    public string? Description { get; init; }
}

public sealed class CreateTestValidator : Validator<CreateTestRequest>
{
    public CreateTestValidator()
    {
        RuleFor(x => x.Id).MustBeIdOfType("test").When(x => x.Id.HasValue);
        RuleFor(x => x.Name).NotNullOrEmptyWithCode().MaximumLengthWithCode(100);
        RuleFor(x => x.Description).MaximumLengthWithCode(100);
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
        var id = req.Id?.Decode() ?? TypeIdDecoded.New("test");
        var test = new Test { Id = id, Name = req.Name };
        _dbContext.Tests.Add(test);
        await _dbContext.SaveChangesAsync(ct);
        Response = new TestDto(
            test.Id.Encode(),
            test.Name,
            test.Description,
            _versionEncoder.Encode(test.Version),
            _clock.GetCurrentInstant()
        );
    }
}
