using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CodingChallenge.Application.TvShow.Services;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using Newtonsoft.Json;

namespace CodingChallenge.Infrastructure.Services;

public class MessagePublisher : IMessagePublisher
{
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly string _topicArn;

    // Move to settings
    public const string EventTopicSuffix = "eventtopic";
    private const string TopicArnFormat = "arn:aws:sns:us-east-1:662912956137:{0}";

    public MessagePublisher(AWSAppProject awsApplication)
    {
        _snsClient = new AmazonSimpleNotificationServiceClient();

        _topicArn = string.Format(TopicArnFormat, awsApplication.GetResourceName(EventTopicSuffix));
    }

    public async Task<bool> PublishAsync<T>(T message, CancellationToken cancellationToken)
    {
        var publishRequest = new PublishRequest
        {
            Message = JsonConvert.SerializeObject(message),
            TopicArn = _topicArn
        };

        await _snsClient.PublishAsync(publishRequest, cancellationToken);

        return true;
    }
}