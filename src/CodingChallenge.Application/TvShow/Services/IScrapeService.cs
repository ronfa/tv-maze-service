using CodingChallenge.Application.Response;
using CodingChallenge.Application.TvShow.Commands.AddScrapeTask;

namespace CodingChallenge.Application.TvShow.Services;

public interface IScrapeService
{
    Task<bool> AddScrapeTaskAsync(AddScrapeTaskCommand command, CancellationToken cancellationToken);

    Task<TvShowDataResponse> ScrapeAsync(int index, CancellationToken cancellationToken);
}