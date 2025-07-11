using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FastEndpoints;
using HackathonManager.Features.Users;
using HackathonManager.Features.Users.CreateUser;
using HackathonManager.Persistence;
using HackathonManager.Persistence.Entities;
using HackathonManager.Persistence.Enums;
using HackathonManager.Services;
using HackathonManager.Tests.TestInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Shouldly;
using Sqids;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests.Users;

public class CreateUserTests : IntegrationTestWithReset
{
    /// <inheritdoc />
    public CreateUserTests(IntegrationTestWithResetFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task ShouldReturn200AndCreateUser()
    {
        // arrange
        var expected = new CreateUserRequest
        {
            Email = Faker.Internet.Email(),
            DisplayName = Faker.Name.FullName(),
            Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
        };

        using var client = App.CreateClient();

        // act
        var (response, actual) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(expected);

        // assert
        using var scope = App.Services.CreateScope();
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var dbContext = scope.ServiceProvider.GetRequiredService<HackathonDbContext>();
        var users = await dbContext.Users.ToArrayAsync(CancellationToken);
        var user = users.ShouldHaveSingleItem();
        user.Id.ShouldBe(actual.Id.Decode());
        scope
            .ServiceProvider.GetRequiredService<PasswordService>()
            .VerifyPassword(expected.Password, user.PasswordHash)
            .ShouldBeTrue();

        var auditEvents = await dbContext
            .UserAuditEvents.Where(x => x.UserId == user.Id)
            .ToArrayAsync(CancellationToken);
        auditEvents
            .ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.UserId.ShouldBe(user.Id),
                x => x.Event.ShouldBe(UserAuditEventType.Registration),
                x =>
                    x.GetMetadata<UserRegistrationMetadataV1>()
                        .ShouldBe(new UserRegistrationMetadataV1(expected.Email, expected.DisplayName))
            );

        var expectedETag = scope.ServiceProvider.GetRequiredService<SqidsEncoder<uint>>().Encode(user.RowVersion);
        response.Headers.GetValues(HeaderNames.ETag).ShouldHaveSingleItem().ShouldBe(expectedETag);

        actual.ShouldSatisfyAllConditions(
            x => x.Id.Type.ToString().ShouldBe(ResourceTypes.User),
            x => x.Email.ShouldBe(expected.Email),
            x => x.Name.ShouldBe(expected.DisplayName)
        );
    }

    [Fact]
    public async Task ShouldReturn409_WhenDisplayNameAlreadyExists()
    {
        // arrange
        using var client = App.CreateClient();

        var displayName = Faker.Name.FullName();
        var seedUserResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = displayName,
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );
        seedUserResponse.EnsureSuccessStatusCode();

        // act
        var (response, error) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ProblemDetails>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = displayName,
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );

        // assert
        using var scope = App.Services.CreateScope();
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        error
            .Errors.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.Code.ShouldBe(ErrorCodes.UniqueValueRequired),
                x => x.Name.ShouldBe(nameof(CreateUserRequest.DisplayName), StringCompareShould.IgnoreCase)
            );

        scope.ServiceProvider.GetRequiredService<HackathonDbContext>().Users.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ShouldReturn409_WhenEmailAlreadyExists()
    {
        // arrange
        using var client = App.CreateClient();

        var email = Faker.Internet.Email();
        var seedUserResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
            new CreateUserRequest
            {
                Email = email,
                DisplayName = Faker.Name.FullName(),
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );
        seedUserResponse.EnsureSuccessStatusCode();

        // act
        var (response, error) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ProblemDetails>(
            new CreateUserRequest
            {
                Email = email,
                DisplayName = Faker.Name.FullName(),
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        error
            .Errors.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.Code.ShouldBe(ErrorCodes.UniqueValueRequired),
                x => x.Name.ShouldBe(nameof(CreateUserRequest.Email), StringCompareShould.IgnoreCase)
            );

        using var scope = App.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<HackathonDbContext>().Users.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task ShouldReturn422_WhenInputsAreNotValid()
    {
        // arrange
        using var client = App.CreateClient();

        // act
        var (response, error) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ProblemDetails>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = Faker.Random.AlphaNumeric(User.DisplayNameMaxLength + 1),
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );

        // assert
        using var scope = App.Services.CreateScope();
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        error
            .Errors.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.Code.ShouldBe(ErrorCodes.InvalidLength),
                x => x.Name.ShouldBe(nameof(CreateUserRequest.DisplayName), StringCompareShould.IgnoreCase)
            );

        scope.ServiceProvider.GetRequiredService<HackathonDbContext>().Users.ShouldBeEmpty();
    }
}
