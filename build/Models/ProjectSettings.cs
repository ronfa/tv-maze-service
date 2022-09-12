namespace Build.Models;

public class ProjectSettings
{
    public string ProjectName { get; set; } = string.Empty;
    public bool PublishProject { get; set; }

    public bool RunCdkDeploy { get; set; }
    public IEnumerable<DockerImageSetting> DockerImageSettings {get;set;} = Enumerable.Empty<DockerImageSetting>();

    public class DockerImageSetting
    {
        public bool RunDockerBuild { get; set; }
        public bool RunDockerPush { get; set; }
        public string DockerRepoNameSuffix { get; set; } = string.Empty;
        public string DockerFileName {get;set;} = string.Empty;
        public string AwsAccountNumber {get;set;} = string.Empty;
        public string AwsRegion {get;set;} = string.Empty;
    }
}