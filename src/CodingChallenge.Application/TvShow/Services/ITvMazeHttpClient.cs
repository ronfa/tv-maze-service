using RestSharp;

namespace CodingChallenge.Application.TvShow.Services;

public interface ITvMazeHttpClient
{
    public Task<RestResponse> GetAsync(int id, CancellationToken cancellationToken);
}