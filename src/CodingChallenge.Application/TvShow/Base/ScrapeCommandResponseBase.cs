namespace CodingChallenge.Application.TvShow.Base;

public abstract record ScrapeCommandResponseBase
{
    public bool IsSuccess => string.IsNullOrWhiteSpace(ErrorMessage);

    public string ErrorMessage { get; internal set; } = string.Empty;
}