namespace CodingChallenge.Application.TvShow.Services;

public interface IMessagePublisher
{
    public Task<bool> PublishAsync<T>(T message, CancellationToken cancellationToken);
}