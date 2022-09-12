using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SQS;
using CodingChallenge.Infrastructure;
using CodingChallenge.Infrastructure.Extensions;
using CodingChallenge.Infrastructure.Models;
using static Amazon.CDK.AWS.APIGateway.CfnDomainName;

namespace CodingChallenge.Cdk.Extensions;

public static class AwsApplicationExtensions
{
    public static string GetDefaultMainStackName(this AWSAppProject awsApplication)
    {
        return awsApplication.GetResourceName(Constants.MAIN_STACK_NAME_SUFFIX);
    }

    public static string GetDefaultInfraStackName(this AWSAppProject awsApplication)
    {
        return awsApplication.GetResourceName(Constants.INFRA_STACK_NAME_SUFFIX);
    }

    public static string GetDefaultDatabaseStackName(this AWSAppProject awsApplication)
    {
        return awsApplication.GetResourceName(Constants.DATABASE_STACK_NAME_SUFFIX);
    }

    public static string GetDefaultApiStackName(this AWSAppProject awsApplication)
    {
        return awsApplication.GetResourceName(Constants.API_STACK_NAME_SUFFIX);
    }

    public static Queue GetSqsQueue<T>(this AWSAppProject awsApplication, T stack, string resourceSuffix,
        bool isFifo = false, IDeadLetterQueue? deadLetterQueue = null)
        where T : Stack
    {
        var resourceName = awsApplication.GetResourceName(resourceSuffix);
        if (isFifo) resourceName += ".fifo";
        var props = new QueueProps
        {
            QueueName = resourceName,
            RetentionPeriod = Duration.Days(10),
            VisibilityTimeout = Duration.Seconds(30),
            DeliveryDelay = Duration.Seconds(20),
            ReceiveMessageWaitTime = Duration.Seconds(0),
            DeadLetterQueue = deadLetterQueue
        };
        if (isFifo)
        {
            props.ContentBasedDeduplication = true;
            props.Fifo = isFifo;
        }

        var resource = new Queue(stack, resourceName, props);
        return resource;
    }

    public static Role GetLambdaRole<T>(this AWSAppProject awsApplication, T stack, string resourceSuffix,
        bool isGlobal = false)
        where T : Stack
    {
        var resourceName = awsApplication.GetResourceName(resourceSuffix);
        var role = new Role(stack, resourceName, new RoleProps
        {
            RoleName = resourceName,
            AssumedBy = new ServicePrincipal("lambda.amazonaws.com")
        });
        return role;
    }


    public static Role GetAPIGatewayRole<T>(this AWSAppProject awsApplication, T stack, string resourceSuffix,
        bool isGlobal = false)
        where T : Stack
    {
        var resourceName = awsApplication.GetResourceName(resourceSuffix);
        var role = new Role(stack, resourceName, new RoleProps
        {
            RoleName = resourceName,
            AssumedBy = new ServicePrincipal("apigateway.amazonaws.com")
        });
        return role;
    }


    public static string SetCfOutput(this AWSAppProject awsApp, Stack stack, string exportSuffix, string exportValue)
    {
        var exportName = awsApp.ConstructCfExportName(exportSuffix);
        var cfOutputBaseMappindgProps = new CfnOutputProps
        {
            ExportName = exportName,
            Value = exportValue
        };
        _ = new CfnOutput(stack, exportName, cfOutputBaseMappindgProps);
        return exportName;
    }

    public static string GetCfOutput(this AWSAppProject awsApp, string exportSuffix)
    {
        var exportName = awsApp.ConstructCfExportName(exportSuffix);
        return Fn.ImportValue(exportName);
    }

    public static string ConstructCfExportName(this AWSAppProject awsApp, string exportSuffix)
    {
        return awsApp.GetResourceName(exportSuffix);
    }

    public static string SetApiGatewayCNameOutputValue(this AWSAppProject awsApp, Stack stack, string outputValue)
    {
        return awsApp.SetCfOutput(stack, Constants.REST_API_CNAME, outputValue);
    }


    public static Dictionary<string, string> GetEnvironmentVariables(this AWSAppProject awsApplication, string prefix)
    {
        return new Dictionary<string, string>
        {
            {prefix + "Environment", awsApplication.Environment},
            {prefix + "Platform", awsApplication.Platform},
            {prefix + "System", awsApplication.System},
            {prefix + "Subsystem", awsApplication.Subsystem},
            {prefix + "Version", awsApplication.Version},
            {prefix + "AwsRegion", awsApplication.AwsRegion}
        };
    }

    public static ICertificate GetCertificate(this AWSAppProject awsApp, Stack stack)
    {
        if (string.IsNullOrWhiteSpace(awsApp.CertificateArn))
            throw new ArgumentNullException(nameof(awsApp.CertificateArn));
        return Certificate.FromCertificateArn(stack, awsApp.GetResourceName("certificate"), awsApp.CertificateArn);
    }

    public static void SetupApiGatewayDomain(this AWSAppProject awsApp, Stack stack, string subdomain, string restApiId,
        string webapiResourceSuffix = "webapi")
    {
        //var restApiId = Fn.ImportValue(awsApp.ConstructCfExportName(webapiResourceSuffix.GetApiGatewayRestApiExportKeySuffix()));
        //var restApiReference = RestApi.FromRestApiId(stack,awsApp.GetResourceName($"{webapiResourceSuffix}-restapiref"),restApiId);
        var certificate = awsApp.GetCertificate(stack);
        var domainEnvironmentSuffix = "";
        if (awsApp.Environment.ToLower() != "prod") domainEnvironmentSuffix = $"-{awsApp.Environment}";
        var domainNameFull = $"{subdomain}{domainEnvironmentSuffix}.{awsApp.DomainName}";

        var domainNameObj = new CfnDomainName(stack, awsApp.GetResourceName($"{webapiResourceSuffix}-domainname"),
            new CfnDomainNameProps
            {
                //Certificate = certificate,
                RegionalCertificateArn = certificate.CertificateArn,
                DomainName = domainNameFull,
                EndpointConfiguration = new EndpointConfigurationProperty
                {
                    Types = new[] {"REGIONAL"}
                }
            });
        if (domainNameObj == null) throw new NullReferenceException();
        var basePathMapping = new CfnBasePathMapping(stack,
            awsApp.GetResourceName($"{webapiResourceSuffix}-basemappings"), new CfnBasePathMappingProps
            {
                DomainName = domainNameObj?.DomainName ?? "domainname",
                RestApiId = restApiId,
                Stage = awsApp.Environment
            });
        if (domainNameObj != null)
            basePathMapping.AddDependsOn(domainNameObj);


        // awsApp.SetCfOutput(stack, $"{webapiResourceSuffix.GetApiGatewayInternalDomainExportKeySuffix()}", domainNameObj.AttrRegionalDomainName);
        // awsApp.SetCfOutput(stack, $"{webapiResourceSuffix.GetApiGatewayExternalDomainExportKeySuffix()}", domainNameObj.DomainName);
        // awsApp.SetCfOutput(stack, $"{webapiResourceSuffix.GetApiGatewaySubdomainExportKeySuffix()}", subdomain);
    }
}