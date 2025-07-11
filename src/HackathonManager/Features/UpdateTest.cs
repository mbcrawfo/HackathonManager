using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using FastEndpoints;
using FastEndpoints.AspVersioning;
using FastIDs.TypeId;
using FluentValidation;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using HackathonManager.Utilities.JsonPatch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Sqids;

namespace HackathonManager.Features;

public sealed class UpdateTestRequest : PatchRequest<Test>
{
    /// <summary>
    ///     Id of the test resource.
    /// </summary>
    [RouteParam]
    public required TypeId Id { get; init; }

    [FromHeader(IsRequired = false, HeaderName = "If-Match")]
    public string? IfMatch { get; init; }
}

public sealed class UpdateTestRequestValidator : PatchRequestValidator<UpdateTestRequest, Test>
{
    public UpdateTestRequestValidator(SqidsEncoder<uint> encoder)
    {
        RuleFor(x => x.Id).MustBeIdOfType("test");
        RuleFor(x => x.IfMatch!)
            .NotEmptyWithCode()
            .MaximumLengthWithCode(Constants.ETagMaxLength)
            .When(x => x.IfMatch is not null);
    }
}

public class UpdateTest : Endpoint<UpdateTestRequest, Results<Ok<TestDto>, NotFound, ProblemDetails>>
{
    private readonly IClock _clock;
    private readonly HackathonDbContext _dbContext;
    private readonly SqidsEncoder<uint> _versionEncoder;

    public UpdateTest(HackathonDbContext dbContext, SqidsEncoder<uint> versionEncoder, IClock clock)
    {
        _dbContext = dbContext;
        _versionEncoder = versionEncoder;
        _clock = clock;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        Patch("/test/{id}");
        AllowAnonymous();
        Description(x =>
            x.Accepts<UpdateTestRequest>("application/json-patch+json")
                .Produces<ProblemDetails>(
                    StatusCodes.Status412PreconditionFailed,
                    ProblemDetailsDefaults.MediaType.Json
                )
        );
        Options(x => x.WithVersionSet("Test").MapToApiVersion(1.0));
    }

    /// <inheritdoc />
    public override async Task<Results<Ok<TestDto>, NotFound, ProblemDetails>> ExecuteAsync(
        UpdateTestRequest req,
        CancellationToken ct
    )
    {
        var test = await _dbContext.Tests.FirstOrDefaultAsync(t => t.Id == req.Id.Decode(), ct);
        if (test is null)
        {
            return TypedResults.NotFound();
        }

        if (
            req.IfMatch is not null
            && (_versionEncoder.Decode(req.IfMatch) is not [var requiredVersion] || requiredVersion != test.Version)
        )
        {
            return new ProblemDetails
            {
                Detail = "ETag was not matched",
                Status = StatusCodes.Status412PreconditionFailed,
            };
        }

        req.ApplyOperationsTo(test);
        await _dbContext.SaveChangesAsync(ct);

        return TypedResults.Ok(
            new TestDto(
                test.Id.Encode(),
                test.Name,
                test.Description,
                _versionEncoder.Encode(test.Version),
                _clock.GetCurrentInstant()
            )
        );
    }
}
