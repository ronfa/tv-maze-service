using AutoFixture;
using CodingChallenge.Application.Interfaces;
using CodingChallenge.Application.TvShow.Queries;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CodingChallenge.Application.UnitTests.TvMaze.Queries;

public class GetItemsQueryTests
{
    private readonly Fixture _fixture;
    private readonly IReadOnlyTvMazeRepository _repo;

    private GetItemListQueryHandler _handler;

    public GetItemsQueryTests()
    {
        _fixture = new();
        _repo = Substitute.For<IReadOnlyTvMazeRepository>();

        _handler = new GetItemListQueryHandler(_repo);
    }

    [Fact]
    public async Task GetItemListQuery()
    {
        var request = _fixture.Create<GetItemListQuery>();

        var data = new Tuple<List<Domain.TvShow.TvShowEntity>, string>(
            _fixture.Create<List<Domain.TvShow.TvShowEntity>>(), _fixture.Create<string>());

        _repo.GetItemListAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(data);

        var response = await _handler.Handle(request, CancellationToken.None);

        response.Should().BeOfType<PagedList<Domain.TvShow.TvShowEntity>>();
        response.Should().NotBeNull();
        response.Items!.Count.Should().Be(data.Item1.Count);
        response.PaginationToken.Should().Be(data.Item2);

        await _repo
            .Received(1)
            .GetItemListAsync(Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}