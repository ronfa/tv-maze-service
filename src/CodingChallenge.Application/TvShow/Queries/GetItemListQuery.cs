using CodingChallenge.Application.Interfaces;
using MediatR;

namespace CodingChallenge.Application.TvShow.Queries;

public record GetItemListQuery( int PageSize,string? PaginationToken=null) : IRequest<PagedList<Domain.TvShow.TvShowEntity>>;

public class GetItemListQueryHandler : IRequestHandler<GetItemListQuery, PagedList<Domain.TvShow.TvShowEntity>>
{
    private readonly IReadOnlyTvMazeRepository repo;

    public GetItemListQueryHandler(IReadOnlyTvMazeRepository context)
    {
        repo = context;
    }

    public async Task<PagedList<Domain.TvShow.TvShowEntity>> Handle(GetItemListQuery request, CancellationToken cancellationToken)
    {
        var responseEntity = await repo.GetItemListAsync(request.PageSize,request.PaginationToken);

        var sorted = responseEntity.Item1
            .OrderBy(s => s.Id).ThenBy(
                s => s.Cast.OrderByDescending(c => c.BirthDate)).ToList();

        return new PagedList<Domain.TvShow.TvShowEntity>(sorted, sorted.Count,responseEntity.Item2);
    }
}
