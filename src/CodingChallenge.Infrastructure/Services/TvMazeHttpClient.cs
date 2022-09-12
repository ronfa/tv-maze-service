using CodingChallenge.Application.TvShow.Services;
using RestSharp;

namespace CodingChallenge.Infrastructure.Services;

public class TvMazeHttpClient : ITvMazeHttpClient
{
    private readonly RestClient _client;

    // Move to settings
    private const string BaseUrl = "https://api.tvmaze.com";
    private const string GetShowPath = "shows/{0}?embed=cast";

    public TvMazeHttpClient()
    {
        _client = new RestClient(new RestClientOptions(BaseUrl)
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        });
    }

    public async Task<RestResponse> GetAsync(int id, CancellationToken cancellationToken)
    {
        var request = new RestRequest(string.Format(GetShowPath, id));

        return await _client.GetAsync(request, cancellationToken);
    }
}