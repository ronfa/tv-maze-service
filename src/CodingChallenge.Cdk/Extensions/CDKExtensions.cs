using Amazon.CDK;
using Amazon.CDK.AWS.SAM;
using CodingChallenge.Infrastructure.Models;
using static Amazon.CDK.AWS.SAM.CfnFunction;

namespace CodingChallenge.Cdk.Extensions;

public static class CdkExtensions
{
    public static AWSAppProject GetAWSApplication(this App app)
    {
        var awsApplication = new AWSAppProject
        {
            Platform = app.Node.TryGetContext("Platform").ToString()!,
            System = app.Node.TryGetContext("System").ToString()!,
            Subsystem = app.Node.TryGetContext("Subsystem").ToString()!,
            Environment = app.Node.TryGetContext("Environment").ToString()!,
            Version = app.Node.TryGetContext("Version").ToString()!,
            CertificateArn = app.Node.TryGetContext("CertificateArn").ToString()!,
            DomainName = app.Node.TryGetContext("DomainName").ToString()!
        };
        return awsApplication;
    }
    public static EventSourceProperty GetWebApiEventSourceProperty(this CfnApi webapi, string method, string path)
    {
        return new EventSourceProperty()
        {
            Type = "Api",
            Properties = new ApiEventProperty()
            {
                Method = method,
                Path = path,
                RestApiId = webapi.Ref
            }
        };
    }

   
}
