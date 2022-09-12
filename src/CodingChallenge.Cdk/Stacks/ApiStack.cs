using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SAM;
using CodingChallenge.Api;
using CodingChallenge.Cdk.Extensions;
using CodingChallenge.Cdk.Models;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using CfnFunction = Amazon.CDK.AWS.SAM.CfnFunction;

namespace CodingChallenge.Cdk.Stacks;

public class ApiStack : Stack
{
    public const string RestApiOutputSuffix = "apistackbase";
    public const string ApiSubdomainName = "tvmazeapi";

    public ApiStack(Construct parent, string id, IStackProps props, AWSAppProject awsApp) : base(parent, id, props)
    {
        var roleArn = GetLambdaRole(awsApp, "apilambdaRole").RoleArn;
        var restApi = GetWebApi(awsApp);

        //SetupApiGateway(awsApp, restApi);
        SetupApiLambda(roleArn, awsApp, restApi);

        // TODO - make this a separate stack. This line needs to run after restapi is created.
        awsApp.SetupApiGatewayDomain(this, ApiSubdomainName, restApi.Ref);
    }


    private CfnFunction SetupApiLambda(string roleArn, AWSAppProject awsApplication, CfnApi restApi)
    {
        var repoUri = GetApiDockerImageUri(awsApplication);
        var functionReference = nameof(LambdaEntryPoint).ToLower();

        var baseSettings = new LamdaFunctionCdkSettings
        {
            FunctionNameSuffix = functionReference,
            Memory = 1024,
            ReservedConcurrentExecutions = 10,
            Timeout = 30,
            RoleArn = roleArn,
            ImageUri = repoUri,
            Architectures = new[] {Architecture.ARM_64.Name},
            HandlerFunctionName = "FunctionHandlerAsync",
            HandlerClassType = typeof(LambdaEntryPoint)
        };
        var lambdaProp = baseSettings.GetLambdaContainerBaseProps(awsApplication);

        lambdaProp.AddEventSourceProperty(restApi.GetWebApiEventSourceProperty("Any", "/{proxy+}"), "any");

        return new CfnFunction(this, lambdaProp.FunctionName ?? functionReference, lambdaProp);
    }

    private string GetDockerImageUri(AWSAppProject awsApplication)
    {
        var dockerImageName = awsApplication.GetCfOutput($"{InfraStack.GetListEcrRepoSuffix}-name");
        var dockerImageEcrDomain = $"{Account}.dkr.ecr.{Region}.amazonaws.com";
        return $"{dockerImageEcrDomain}/{dockerImageName}:{awsApplication.Version}";
    }

    private string GetApiDockerImageUri(AWSAppProject awsApplication)
    {
        var dockerImageName = awsApplication.GetCfOutput($"{InfraStack.ApiRepoSuffix}-name");
        var dockerImageEcrDomain = $"{Account}.dkr.ecr.{Region}.amazonaws.com";
        return $"{dockerImageEcrDomain}/{dockerImageName}:{awsApplication.Version}";
    }

    public Role GetLambdaRole(AWSAppProject awsApplication, string namesuffix)
    {
        return awsApplication.GetLambdaRole(this, namesuffix)
            .AddCodeBuildReportGroupPolicy(awsApplication)
            .AddCloudWatchLogsPolicy(awsApplication)
            .AddCloudWatchLogGroupPolicy(awsApplication)
            .AddDynamoDBPolicy(awsApplication)
            .AddSnsPolicy(awsApplication)
            .AddSqsPolicy(awsApplication)
            .AddS3Policy(awsApplication)
            .AddSsmPolicy(awsApplication);
    }

    private CfnApi GetWebApi(AWSAppProject awsApplication)
    {
        var apiResourceName = awsApplication.GetResourceName("listapi");
        var apiProps = new CfnApiProps
        {
            Name = apiResourceName,
            StageName = awsApplication.Environment,
            EndpointConfiguration = "REGIONAL"
        };
        var api = new CfnApi(this, apiResourceName, apiProps);
        awsApplication.SetCfOutput(this, RestApiOutputSuffix, api.Ref);
        return api;
    }
}