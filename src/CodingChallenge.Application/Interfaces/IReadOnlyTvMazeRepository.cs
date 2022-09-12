using CodingChallenge.Domain.TvShow;

namespace CodingChallenge.Application.Interfaces;

public interface IReadOnlyTvMazeRepository
{
    Task<TvShowEntity?> GetByIndexAsync(int index, CancellationToken cancellationToken);

    Task<Tuple<List<TvShowEntity>, string>> GetItemListAsync(int pageSize, string? paginationToken = null,
        CancellationToken cancellationToken = default);
}