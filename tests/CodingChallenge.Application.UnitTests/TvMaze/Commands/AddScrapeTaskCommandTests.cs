using AutoFixture;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;
using CodingChallenge.Application.TvShow.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace CodingChallenge.Application.UnitTests.TvMaze.Commands;

public class AddScrapeTaskCommandTests
{
    private readonly Fixture _fixture;
    private readonly IScrapeService _scrapeService;

    private AddScrapeTaskCommandHandler _handler;

    public AddScrapeTaskCommandTests()
    {
        _fixture = new();
        _scrapeService = Substitute.For<IScrapeService>();

        _handler = new AddScrapeTaskCommandHandler(_scrapeService,
            Substitute.For<ILogger<AddScrapeTaskCommandHandler>>());
    }

    [Fact]
    public async Task AddScrapeTaskCommand()
    {
        var request = _fixture.Create<AddScrapeTaskCommand>();

        _scrapeService.AddScrapeTaskAsync(Arg.Any<AddScrapeTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<AddScrapeTaskCommandResponse>();

        response.StartIndex.Should().Be(request.StartIndex);
        response.EndIndex.Should().Be(request.EndIndex);

        await _scrapeService
            .Received(1)
            .AddScrapeTaskAsync(Arg.Any<AddScrapeTaskCommand>(), Arg.Any<CancellationToken>());
    }
}
