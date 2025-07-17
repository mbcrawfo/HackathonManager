using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FastEndpoints;
using HackathonManager.Features.Users.CreateUser;
using HackathonManager.Features.Users.GetUsers;
using HackathonManager.Tests.TestInfrastructure;
using Microsoft.Net.Http.Headers;
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
        response.Headers.GetValues(HeaderNames.ETag).ShouldHaveSingleItem();
        result.Users.Select(u => u.Name).ShouldBe(expectedUserNames);
        result.PrevCursor.ShouldBeNull();
        result.NextCursor.ShouldBeNull();
    }

    [Theory]
    [InlineData(UserSort.Name)]
    [InlineData(UserSort.NameDesc)]
    [InlineData(UserSort.Email)]
    [InlineData(UserSort.EmailDesc)]
    [InlineData(UserSort.Created)]
    [InlineData(UserSort.CreatedDesc)]
    public async Task ShouldReturnCorrectlySortedUsers(UserSort sort)
    {
        // arrange
        using var client = App.CreateClient();

        var users = new[]
        {
            new { Name = "Alice", Email = "alice@example.com" },
            new { Name = "Bob", Email = "bob@example.com" },
            new { Name = "Charlie", Email = "charlie@example.com" },
        };

        foreach (var user in Faker.Random.Shuffle(users))
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = user.Email,
                    DisplayName = user.Name,
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act
        var (response, result) = await client.GETAsync<GetUsersEndpoint, GetUsersRequest, GetUsersResponseDto>(
            new GetUsersRequest(Search: null, Term: null, Sort: sort)
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Users.Count().ShouldBe(3);

        var resultUsers = result.Users.ToArray();
        switch (sort)
        {
            case UserSort.Name:
                resultUsers.Select(u => u.Name).ShouldBe(["Alice", "Bob", "Charlie"]);
                break;
            case UserSort.NameDesc:
                resultUsers.Select(u => u.Name).ShouldBe(["Charlie", "Bob", "Alice"]);
                break;
            case UserSort.Email:
                resultUsers
                    .Select(u => u.Email)
                    .ShouldBe(["alice@example.com", "bob@example.com", "charlie@example.com"]);
                break;
            case UserSort.EmailDesc:
                resultUsers
                    .Select(u => u.Email)
                    .ShouldBe(["charlie@example.com", "bob@example.com", "alice@example.com"]);
                break;
            case UserSort.Created:
            case UserSort.CreatedDesc:
                resultUsers.Length.ShouldBe(3);
                break;
        }
    }

    [Theory]
    [InlineData(UserSearch.Name, "Alice")]
    [InlineData(UserSearch.Name, "Bob")]
    [InlineData(UserSearch.Email, "alice@example.com")]
    [InlineData(UserSearch.Email, "bob@example.com")]
    public async Task ShouldReturnFilteredUsers_WhenSearchParametersProvided(UserSearch search, string term)
    {
        // arrange
        using var client = App.CreateClient();

        var users = new[]
        {
            new { Name = "Alice", Email = "alice@example.com" },
            new { Name = "Bob", Email = "bob@example.com" },
            new { Name = "Charlie", Email = "charlie@example.com" },
        };

        foreach (var userData in users)
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = userData.Email,
                    DisplayName = userData.Name,
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act
        var (response, result) = await client.GETAsync<GetUsersEndpoint, GetUsersRequest, GetUsersResponseDto>(
            new GetUsersRequest(Search: search, Term: term)
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Users.ShouldHaveSingleItem();

        var user = result.Users.First();
        if (search == UserSearch.Name)
        {
            user.Name.ShouldBe(term);
        }
        else
        {
            user.Email.ShouldBe(term);
        }
    }

    [Fact]
    public async Task ShouldReturnPaginatedResults_WhenPageSizeIsSmall()
    {
        // arrange
        using var client = App.CreateClient();

        for (int i = 0; i < 3; i++)
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = $"user{i}@example.com",
                    DisplayName = $"User {i}",
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act
        var (response, result) = await client.GETAsync<GetUsersEndpoint, GetUsersRequest, GetUsersResponseDto>(
            new GetUsersRequest(Search: null, Term: null, PageSize: 2)
        );

        // assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.Users.Count().ShouldBe(2);
        result.PrevCursor.ShouldBeNull();
        result.NextCursor.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldReturnNextPageCorrectly_WhenUsingCursor()
    {
        // arrange
        using var client = App.CreateClient();

        var userNames = new[] { "Alice", "Bob", "Charlie", "David", "Eve" };
        foreach (var name in userNames)
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = $"{name.ToLower()}@example.com",
                    DisplayName = name,
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act - get first page
        var (firstResponse, firstResult) = await client.GETAsync<
            GetUsersEndpoint,
            GetUsersRequest,
            GetUsersResponseDto
        >(new GetUsersRequest(Search: null, Term: null, PageSize: 2));

        // act - get second page using cursor
        var (secondResponse, secondResult) = await client.GETAsync<
            GetUsersEndpoint,
            GetUsersRequest,
            GetUsersResponseDto
        >(new GetUsersRequest(Search: null, Term: null, PageSize: 2, Cursor: firstResult.NextCursor));

        // assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstResult.Users.Count().ShouldBe(2);
        firstResult.NextCursor.ShouldNotBeNull();

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResult.Users.Count().ShouldBe(2);
        secondResult.PrevCursor.ShouldBe(firstResult.NextCursor);

        // Ensure no overlap between pages
        var firstPageNames = firstResult.Users.Select(u => u.Name).ToArray();
        var secondPageNames = secondResult.Users.Select(u => u.Name).ToArray();
        firstPageNames.Intersect(secondPageNames).ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldPreserveCursorParameters_WhenNavigatingPages()
    {
        // arrange
        using var client = App.CreateClient();

        var users = new[]
        {
            new { Name = "Alice", Email = "alice@example.com" },
            new { Name = "Bob", Email = "bob@example.com" },
            new { Name = "Charlie", Email = "charlie@example.com" },
        };

        foreach (var userData in users)
        {
            var createResponse = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest>(
                new CreateUserRequest
                {
                    Email = userData.Email,
                    DisplayName = userData.Name,
                    Password = Faker.Random.AlphaNumeric(Constants.PasswordMinLength),
                }
            );
            createResponse.EnsureSuccessStatusCode();
        }

        // act - get first page with search and sort parameters
        var (firstResponse, firstResult) = await client.GETAsync<
            GetUsersEndpoint,
            GetUsersRequest,
            GetUsersResponseDto
        >(new GetUsersRequest(Search: UserSearch.Name, Term: "A", Sort: UserSort.NameDesc, PageSize: 1));

        // act - get second page using cursor (should preserve search and sort)
        var (secondResponse, secondResult) = await client.GETAsync<
            GetUsersEndpoint,
            GetUsersRequest,
            GetUsersResponseDto
        >(new GetUsersRequest(Search: null, Term: null, Cursor: firstResult.NextCursor));

        // assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        firstResult.Users.ShouldHaveSingleItem();

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResult.Users.Count().ShouldBeLessThanOrEqualTo(1);
    }
}
