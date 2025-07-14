using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FastEndpoints;
using HackathonManager.Features.Users.CreateUser;
using HackathonManager.Features.Users.GetUsers;
using HackathonManager.Tests.TestInfrastructure;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.IntegrationTests.Users;

public class GetUsersTests : IntegrationTestWithReset
{
    /// <inheritdoc />
    public GetUsersTests(IntegrationTestWithResetFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task ShouldReturn200WithDefaultSort_WhenNoParametersProvided()
    {
        // arrange
        using var client = App.CreateClient();

        string[] expectedUserNames = ["aa", "bb", "cc"];
        foreach (var name in Faker.Random.Shuffle(expectedUserNames))
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = Faker.Internet.Email(),
                    DisplayName = name,
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act
        var (response, result) = await client.GETAsync<GetUsersEndpoint, GetUsersRequest, GetUsersResponseDto>(
            new GetUsersRequest(Search: null, Term: null)
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Users.Select(u => u.Name).ShouldBe(expectedUserNames);
        result.PrevCursor.ShouldBeNull();
        result.NextCursor.ShouldBeNull();
    }
}
