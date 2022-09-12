using Cake.Common.Diagnostics;
using Cake.Docker;
using Cake.Frosting;
using CodingChallenge.Infrastructure.Extensions;

namespace Build.Tasks;

[TaskName("Docker-Build-Task")]
public sealed class DockerBuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.UseDefaultDockerContext();

        foreach (var projectSetting in context.Config.ProjectSettingsList)
        {
            if (projectSetting.DockerImageSettings == null || !projectSetting.DockerImageSettings.Any())
            {
                context.Information("there is no DockerSettingsList");
                continue;
            }

            foreach (var dockerSetting in projectSetting.DockerImageSettings)
            {
                var dockerFolderPath = Path.Combine(context.Config.StandardFolders.PublishDirFullPath,
                    projectSetting.ProjectName);
                //var dockerFilePath = System.IO.Path.Combine(context.Config.StandardFolders.PublishDirFullPath, projectSetting.ProjectName);
                var tagName = context.Config.AwsApplication.GetResourceName(dockerSetting.DockerRepoNameSuffix);
                var dockerBuildSettings = new DockerImageBuildSettings
                {
                    Tag = new[] {$"{tagName}"}
                };
                var dockerFileName = dockerSetting.DockerFileName;

                string? dockerFilePath = null;
                if (!string.IsNullOrWhiteSpace(dockerFileName))
                {
                    dockerBuildSettings.File = Path.Combine(dockerFolderPath, dockerFileName);
                    dockerFilePath = Path.Combine(dockerFolderPath, dockerFileName);
                }

                context.Information($"docker file path is : {dockerFilePath} --- File name : {dockerFileName}");
                //context.DockerBuild(dockerBuildSettings, dockerFilePath);
                context.DockerXBuild(tagName, dockerFolderPath, dockerFilePath, "linux/arm64");
            }
        }
    }
}