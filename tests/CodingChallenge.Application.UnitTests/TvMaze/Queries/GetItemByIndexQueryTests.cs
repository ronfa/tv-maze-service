using AutoFixture;
using CodingChallenge.Application.Interfaces;
using CodingChallenge.Application.TvShow.Queries;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CodingChallenge.Application.UnitTests.TvMaze.Queries;

public class GetItemByIndexQueryTests
{
    private readonly Fixture _fixture;
    private readonly IReadOnlyTvMazeRepository _repo;

    private GetItemByIndexHandler _handler;

    public GetItemByIndexQueryTests()
    {
        _fixture = new();
        _repo = Substitute.For<IReadOnlyTvMazeRepository>();

        _handler = new GetItemByIndexHandler(_repo);
    }

    [Fact]
    public async Task GetItemByIndex()
    {
        var request = _fixture.Create<GetItemByIndexQuery>();

        _repo.GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new Domain.TvShow.TvShowEntity {Id = request.Index});

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<Domain.TvShow.TvShowEntity>();
        response.Should().NotBeNull();
        response!.Id.Should().Be(request.Index);

        await _repo
            .Received(1)
            .GetByIndexAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
