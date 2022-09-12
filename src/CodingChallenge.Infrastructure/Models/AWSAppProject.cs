using CodingChallenge.Domain.Interfaces;

namespace CodingChallenge.Infrastructure.Models;

public class AWSAppProject : IInfrastructureProject
{
    public string AwsRegion { get; set; } = string.Empty;
    public string CertificateArn { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public string Subsystem { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}