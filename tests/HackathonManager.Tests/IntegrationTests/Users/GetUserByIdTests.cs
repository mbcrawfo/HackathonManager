using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FastEndpoints;
using FastIDs.TypeId;
using HackathonManager.Features.Users;
using HackathonManager.Features.Users.CreateUser;
using HackathonManager.Features.Users.GetUserById;
using HackathonManager.Tests.TestInfrastructure;
using Microsoft.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests.Users;

public class GetUserByIdTests : IntegrationTestWithReset
{
    /// <inheritdoc />
    public GetUserByIdTests(IntegrationTestWithResetFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task ShouldReturn200AndUser()
    {
        // arrange
        using var client = App.CreateClient();

        var (seedResponse, expected) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = Faker.Name.FullName(),
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );
        seedResponse.EnsureSuccessStatusCode();

        var expectedETag = seedResponse.Headers.GetValues(HeaderNames.ETag).Single();

        // act
        var (response, actual) = await client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest, UserDto>(
            new GetUserByIdRequest { Id = expected.Id }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues(HeaderNames.ETag).ShouldHaveSingleItem().ShouldBe(expectedETag);

        actual.ShouldSatisfyAllConditions(
            x => x.Id.ShouldBe(expected.Id),
            // Avoids mismatches in timestamp precision between Postgres and NodaTime.
            x => x.Created.ToUnixTimeMilliseconds().ShouldBe(expected.Created.ToUnixTimeMilliseconds()),
            x => x.Email.ShouldBe(expected.Email),
            x => x.Name.ShouldBe(expected.Name),
            x => x.Version.ShouldBe(expected.Version)
        );
    }

    [Fact]
    public async Task ShouldReturn304_WhenUserHasNotBeenModified()
    {
        // arrange
        using var client = App.CreateClient();

        var (seedResponse, expected) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(
            new CreateUserRequest
            {
                Email = Faker.Internet.Email(),
                DisplayName = Faker.Name.FullName(),
                Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
            }
        );
        seedResponse.EnsureSuccessStatusCode();

        var etag = seedResponse.Headers.GetValues(HeaderNames.ETag).Single();

        // act
        var response = await client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest>(
            new GetUserByIdRequest { Id = expected.Id, IfNoneMatch = etag }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task ShouldReturn404_WhenUserDoesNotExist()
    {
        // arrange
        using var client = App.CreateClient();

        // act
        var response = await client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest>(
            new GetUserByIdRequest { Id = TypeId.New(ResourceTypes.User).Encode() }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
