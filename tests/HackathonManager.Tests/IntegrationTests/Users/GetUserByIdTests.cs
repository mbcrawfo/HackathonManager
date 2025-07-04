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

public class GetUserByIdTests : IntegrationTestBase<HackathonApp_DatabaseReset>
{
    /// <inheritdoc />
    public GetUserByIdTests(HackathonApp_DatabaseReset hackathonApp)
        : base(hackathonApp) { }

    [Fact]
    public async Task ShouldReturn200AndUser()
    {
        // arrange
        var (seedResponse, expected) = await App.Client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(
            new CreateUserRequest
            {
                Email = Fake.Internet.Email(),
                DisplayName = Fake.Name.FullName(),
                Password = Fake.Random.AlphaNumeric(10),
            }
        );
        seedResponse.EnsureSuccessStatusCode();

        var expectedETag = seedResponse.Headers.GetValues(HeaderNames.ETag).Single();

        // act
        var (response, actual) = await App.Client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest, UserDto>(
            new GetUserByIdRequest { Id = expected.Id }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.GetValues(HeaderNames.ETag).ShouldHaveSingleItem().ShouldBe(expectedETag);
        actual.ShouldBe(expected);
    }

    [Fact]
    public async Task ShouldReturn304_WhenUserHasNotBeenModified()
    {
        // arrange
        var (seedResponse, expected) = await App.Client.POSTAsync<CreateUserEndpoint, CreateUserRequest, UserDto>(
            new CreateUserRequest
            {
                Email = Fake.Internet.Email(),
                DisplayName = Fake.Name.FullName(),
                Password = Fake.Random.AlphaNumeric(10),
            }
        );
        seedResponse.EnsureSuccessStatusCode();

        var etag = seedResponse.Headers.GetValues(HeaderNames.ETag).Single();

        // act
        var response = await App.Client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest>(
            new GetUserByIdRequest { Id = expected.Id, IfNoneMatch = etag }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task ShouldReturn404_WhenUserDoesNotExist()
    {
        // arrange
        // act
        var response = await App.Client.GETAsync<GetUserByIdEndpoint, GetUserByIdRequest>(
            new GetUserByIdRequest { Id = TypeId.New(ResourceTypes.User).Encode() }
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
