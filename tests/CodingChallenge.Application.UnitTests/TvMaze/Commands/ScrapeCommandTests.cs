using AutoFixture;
using CodingChallenge.Application.Response;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Commands.Scrape;
using CodingChallenge.Application.TvShow.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CodingChallenge.Application.UnitTests.TvMaze.Commands;

public class ScrapeCommandTests
{
    private readonly Fixture _fixture;
    private readonly IScrapeService _scrapeService;

    private ScrapeCommandHandler _handler;

    public ScrapeCommandTests()
    {
        _fixture = new();
        _scrapeService = Substitute.For<IScrapeService>();

        _handler = new ScrapeCommandHandler(_scrapeService,
            Substitute.For<ILogger<AddScrapeTaskCommandHandler>>());
    }

    [Fact]
    public async Task ScrapeIsNotSuccessful()
    {
        var request = _fixture.Create<ScrapeCommand>();

        _scrapeService.ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TvShowDataResponse {IsSuccessful = false});

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<ScrapeCommandResponse>();

        response.IsSuccess.Should().BeFalse();

        await _scrapeService
            .Received(1)
            .ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeNoShowFound()
    {
        var request = _fixture.Create<ScrapeCommand>();

        _scrapeService.ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TvShowDataResponse { IsSuccessful = false, NotFound = true });

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<ScrapeCommandResponse>();

        response.IsSuccess.Should().BeFalse();
        response.NotFound.Should().BeTrue();

        await _scrapeService
            .Received(1)
            .ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeIsRateLimited()
    {
        var request = _fixture.Create<ScrapeCommand>();

        _scrapeService.ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TvShowDataResponse { IsSuccessful = false, RateLimited = true });

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<ScrapeCommandResponse>();

        response.IsSuccess.Should().BeFalse();
        response.RateLimited.Should().BeTrue();

        await _scrapeService
            .Received(1)
            .ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ScrapeSuccessful()
    {
        var request = _fixture.Create<ScrapeCommand>();

        _scrapeService.ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new TvShowDataResponse { IsSuccessful = true });

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<ScrapeCommandResponse>();

        response.IsSuccess.Should().BeTrue();
        response.Index.Should().Be(request.Index);

        await _scrapeService
            .Received(1)
            .ScrapeAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}

