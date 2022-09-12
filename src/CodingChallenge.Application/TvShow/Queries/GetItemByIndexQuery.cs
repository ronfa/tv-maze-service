using CodingChallenge.Application.Interfaces;
using MediatR;

namespace CodingChallenge.Application.TvShow.Queries;

public record GetItemByIndexQuery(int Index) : IRequest<Domain.TvShow.TvShowEntity?>;

public class GetItemByIndexHandler : IRequestHandler<GetItemByIndexQuery, Domain.TvShow.TvShowEntity?>
{
    private readonly IReadOnlyTvMazeRepository _repo;

    public GetItemByIndexHandler(IReadOnlyTvMazeRepository context)
    {
        _repo = context;
    }

    public async Task<Domain.TvShow.TvShowEntity?> Handle(GetItemByIndexQuery request, CancellationToken cancellationToken)
    {
        return await _repo.GetByIndexAsync(request.Index, cancellationToken);
    }
}
