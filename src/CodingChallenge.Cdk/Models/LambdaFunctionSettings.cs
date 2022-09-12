using static Amazon.CDK.AWS.APIGateway.CfnRestApi;

namespace CodingChallenge.Cdk.Models;

public class LamdaFunctionCdkSettings
{
    public string FunctionNameSuffix { get; set; } = string.Empty;  
    public int Memory { get; set; }
    public double ReservedConcurrentExecutions { get; set; }
    public int Timeout { get; set; }

    public Type? HandlerClassType { get; set; }
    public string HandlerFunctionName { get; set; } = string.Empty;  

    public string RoleArn { get; set; } = string.Empty;

    public S3LocationProperty? S3Location { get; set; }
    public string Runtime { get; set; } = string.Empty;
    public string ImageUri { get; set; } = string.Empty;
    public string[] Architectures { get; set; } = Array.Empty<string>();
    public string Authorizer { get; set; } = string.Empty;
}
