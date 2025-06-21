using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using HackathonManager.Settings;
using HackathonManager.Tests.TestInfrastructure.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Serilog.Events;
using Shouldly;
using Xunit;

namespace HackathonManager.Tests.UnitTests;

public class RequestLoggingMiddlewareTests
{
    private static readonly RequestDelegate NextMiddleware = _ => Task.CompletedTask;

    private readonly Faker _faker = new();
    private readonly FakeLogger<RequestLoggingMiddleware> _logger = new();

    private readonly IOptions<RequestLoggingSettings> _logSettingsMock = Substitute.For<
        IOptions<RequestLoggingSettings>
    >();

    [Fact]
    public async Task ShouldCreateLogScopeForRequest()
    {
        // arrange
        _logSettingsMock.Value.Returns(new RequestLoggingSettings());
        var httpContextMock = MockHttpContext();
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger
            .ScopeStates.ShouldHaveSingleItem()
            .ShouldBeAssignableTo<IEnumerable<KeyValuePair<string, object>>>()
            .Select(x => x.Key)
            .ShouldBe(
                [
                    LogProperties.RequestId,
                    LogProperties.RequestMethod,
                    LogProperties.RequestRoute,
                    LogProperties.UrlPath,
                    LogProperties.UrlQuery,
                ],
                ignoreOrder: true
            );
    }

    [Fact]
    public async Task ShouldNotLogRequestBody_WhenLogBodySettingIsFalse()
    {
        // arrange
        _logSettingsMock.Value.Returns(new RequestLoggingSettings { LogBody = false });
        var httpContextMock = MockHttpContext(_faker.Lorem.Sentence(), "text/plain");
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldNotLogRequestBody_WhenBodyLogLevelIsNotEnabled()
    {
        // arrange
        _logger.Level = LogLevel.Information;
        _logSettingsMock.Value.Returns(
            new RequestLoggingSettings { LogBody = true, BodyLogLevel = LogEventLevel.Debug }
        );
        var httpContextMock = MockHttpContext(_faker.Lorem.Sentence(), "text/plain");
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldNotLogRequestBody_WhenRequestBodyIsEmpty()
    {
        // arrange
        _logSettingsMock.Value.Returns(new RequestLoggingSettings { LogBody = true });
        var httpContextMock = MockHttpContext();
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger.Messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldLogRequestBodyContentTypeAndLength_WhenRequestBodyIsNotALoggedType()
    {
        // arrange
        var body = _faker.Lorem.Sentence();
        const string contentType = "text/plain";

        _logSettingsMock.Value.Returns(
            new RequestLoggingSettings { LogBody = true, ContentTypes = ["application/json"] }
        );
        var httpContextMock = MockHttpContext(body, contentType);
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger
            .Messages.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.ShouldContain(contentType),
                x => x.ShouldContain($"{body.Length} bytes")
            );
    }

    [Fact]
    public async Task ShouldLogRequestBodyContentTypeAndLength_WhenRequestBodyIsTooLargeToLog()
    {
        // arrange
        var body = _faker.Lorem.Sentence();
        const string contentType = "text/plain";

        _logSettingsMock.Value.Returns(
            new RequestLoggingSettings
            {
                LogBody = true,
                MaxBodySize = body.Length / 2,
                ContentTypes = ["*/*"],
            }
        );
        var httpContextMock = MockHttpContext(body, contentType);
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger
            .Messages.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.ShouldContain(contentType),
                x => x.ShouldContain($"{body.Length} bytes"),
                x => x.ShouldContain("too large")
            );
    }

    [Fact]
    public async Task ShouldLogRequestBody()
    {
        // arrange
        var body = _faker.Lorem.Sentence();
        const string contentType = "text/plain";

        _logSettingsMock.Value.Returns(new RequestLoggingSettings { LogBody = true, ContentTypes = ["*/*"] });
        var httpContextMock = MockHttpContext(body, contentType);
        var sut = new RequestLoggingMiddleware(NextMiddleware, _logger, _logSettingsMock);

        // act
        await sut.InvokeAsync(httpContextMock);

        // assert
        _logger
            .Messages.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(
                x => x.ShouldContain(contentType),
                x => x.ShouldContain($"{body.Length} bytes"),
                x => x.ShouldContain(body)
            );
    }

    // Verifies that the cast of Serilog level -> MS ILogger LogLevel works as expected.
    [Theory]
    [InlineData(LogEventLevel.Verbose, LogLevel.Trace)]
    [InlineData(LogEventLevel.Debug, LogLevel.Debug)]
    [InlineData(LogEventLevel.Information, LogLevel.Information)]
    [InlineData(LogEventLevel.Warning, LogLevel.Warning)]
    [InlineData(LogEventLevel.Error, LogLevel.Error)]
    [InlineData(LogEventLevel.Fatal, LogLevel.Critical)]
    public void SerilogLogEventLevel_ShouldHaveSameUnderlyingValueAsMicrosoftLogLevel(
        LogEventLevel serilogLevel,
        LogLevel microsoftLevel
    )
    {
        // arrange
        var serilogIntValue = (int)serilogLevel;
        var microsoftIntValue = (int)microsoftLevel;

        // act
        // assert
        serilogIntValue.ShouldBe(microsoftIntValue);
    }

    private HttpContext MockHttpContext(string? requestBody = null, string? contentType = null)
    {
        var mock = Substitute.For<HttpContext>();

        mock.RequestAborted.Returns(CancellationToken.None);
        mock.TraceIdentifier.Returns(_faker.Random.AlphaNumeric(length: 32));
        mock.Request.Method.Returns("POST");

        var uri = new Uri(_faker.Internet.UrlWithPath() + $"?{_faker.Lorem.Word()}={_faker.Lorem.Word()}");
        mock.Request.Scheme.Returns(uri.Scheme);
        mock.Request.Host.Returns(new HostString(uri.Host, uri.Port));
        mock.Request.PathBase.Returns(new PathString(string.Empty));
        mock.Request.Path.Returns(new PathString(uri.AbsolutePath));
        mock.Request.QueryString.Returns(new QueryString(uri.Query));

        if (requestBody is not null)
        {
            var bytes = Encoding.UTF8.GetBytes(requestBody);
            mock.Request.ContentLength = bytes.Length;
            mock.Request.ContentType = contentType;
            mock.Request.Body.Returns(new MemoryStream(bytes));
        }

        mock.Response.StatusCode.Returns(returnThis: 200);

        return mock;
    }
}
