using System.Net;
using System.Threading.Tasks;
using FastEndpoints;
using HackathonManager.Features.Users;
using HackathonManager.Features.Users.CreateUser;
using HackathonManager.Persistence;
using HackathonManager.Persistence.Entities;
using HackathonManager.Tests.TestInfrastructure;
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
            Password = Faker.Random.AlphaNumeric(10),
        };

        using var client = App.CreateClient();

        // act
        var (response, actual) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(expected);

        // assert
        using var scope = App.Services.CreateScope();
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var user = scope.ServiceProvider.GetRequiredService<HackathonDbContext>().Users.ShouldHaveSingleItem();
        user.Id.ShouldBe(actual.Id.Decode());

        var expectedETag = scope.ServiceProvider.GetRequiredService<SqidsEncoder<uint>>().Encode(user.RowVersion);
        response.Headers.GetValues(HeaderNames.ETag).ShouldHaveSingleItem().ShouldBe(expectedETag);

        actual.ShouldSatisfyAllConditions(
            x => x.Id.Type.ToString().ShouldBe(ResourceTypes.User),
            x => x.Email.ShouldBe(expected.Email),
            x => x.DisplayName.ShouldBe(expected.DisplayName)
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
                Password = Faker.Random.AlphaNumeric(10),
            }
        );
        seedUserResponse.EnsureSuccessStatusCode();

        // act
        var (response, error) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ProblemDetails>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = displayName,
                Password = Faker.Random.AlphaNumeric(10),
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
                Password = Faker.Random.AlphaNumeric(10),
            }
        );
        seedUserResponse.EnsureSuccessStatusCode();

        // act
        var (response, error) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ProblemDetails>(
            new CreateUserRequest
            {
                Email = email,
                DisplayName = Faker.Name.FullName(),
                Password = Faker.Random.AlphaNumeric(10),
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
                DisplayName = Faker.Random.AlphaNumeric(User.EmailMaxLength + 1),
                Password = Faker.Random.AlphaNumeric(10),
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
