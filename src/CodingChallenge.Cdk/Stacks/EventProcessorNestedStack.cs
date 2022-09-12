using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using CodingChallenge.Cdk.Extensions;
using CodingChallenge.Cdk.Models;
using CodingChallenge.EventQueueProcessor;
using CodingChallenge.Infrastructure;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using static Amazon.CDK.AWS.SAM.CfnFunction;
using CfnFunction = Amazon.CDK.AWS.SAM.CfnFunction;

namespace CodingChallenge.Cdk.Stacks;

public sealed class EventProcessorNestedStack : NestedStack
{
    public EventProcessorNestedStack(Construct parent, string id, NestedStackProps props, AWSAppProject awsApplication,
        string roleArn, string eventTopicArn) : base(parent, id, props)
    {
        var topicName = awsApplication.GetResourceName("blockChainEventTopic");
        var eventTopic =
            Topic.FromTopicArn(this, awsApplication.GetResourceName("blockChainEventTopic"), eventTopicArn);
        var eventQueueStack = SetupEventTransactionQueue(awsApplication, eventTopic);
        SetupEventProcessorLambdaFunction(roleArn, awsApplication, eventTopic, eventQueueStack.QueueObj);
    }

    private QueueNestedStack SetupEventTransactionQueue(AWSAppProject awsApplication, ITopic eventTopic)
    {
        var blockChainEventQueueNameSuffix = "eventqueue";
        var eventQueueStack = new QueueNestedStack(this, $"{blockChainEventQueueNameSuffix}-stack",
            new NestedStackProps(), awsApplication, blockChainEventQueueNameSuffix, false);
        eventTopic.AddSubscription(new SqsSubscription(eventQueueStack.QueueObj, new SqsSubscriptionProps
        {
            RawMessageDelivery = true
        }));
        return eventQueueStack;
    }

    private CfnFunction SetupEventProcessorLambdaFunction(string roleArn, AWSAppProject awsApplication, ITopic topic,
        Queue eventQueue)
    {
        var repoUri = GetDockerImageUri(awsApplication);

        const string functionSuffix = "event-processor-lambda-func";

        var baseSettings = new LamdaFunctionCdkSettings
        {
            FunctionNameSuffix = functionSuffix.ToLower(),
            Memory = 2048,
            ReservedConcurrentExecutions = 10,
            Timeout = 30,
            RoleArn = roleArn,
            ImageUri = repoUri,
            Architectures = new[] {Architecture.ARM_64.Name},
            HandlerFunctionName = nameof(EventQueueLambdaClass.HandleAsync),
            HandlerClassType = typeof(EventQueueLambdaClass)
        };
        var lambdaProp = baseSettings.GetLambdaContainerBaseProps(awsApplication);

        ((lambdaProp.Environment as FunctionEnvironmentProperty)!.Variables as Dictionary<string, string>)!.Add(
            Constants.DATABASE_TYPE_ENV_VAR_KEY, Constants.DATABASE_TYPE_DYNAMODB_ENV_VAR_KEY);
        lambdaProp.AddEventSourceProperty(eventQueue.GetQueueEventSourceProperty(10, true), "eventKey");

        return new CfnFunction(this, lambdaProp.FunctionName!, lambdaProp);
    }

    private string GetDockerImageUri(AWSAppProject awsApplication)
    {
        var dockerImageName = awsApplication.GetCfOutput($"{InfraStack.EcrRepoSuffix}-name");
        var dockerImageEcrDomain = $"{Account}.dkr.ecr.{Region}.amazonaws.com";
        return $"{dockerImageEcrDomain}/{dockerImageName}:{awsApplication.Version}";
    }
}