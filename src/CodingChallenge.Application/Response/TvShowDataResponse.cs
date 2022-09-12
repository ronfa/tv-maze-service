
using CodingChallenge.Domain.TvShow;

namespace CodingChallenge.Application.Response;

public class TvShowDataResponse
{
    public TvShowEntity? TvShow { get; set; }
    public bool RateLimited { get; set; }
    public bool IsSuccessful { get; set; }
    public bool AlreadyStored { get; set; }
    public bool NotFound { get; set; }
}

