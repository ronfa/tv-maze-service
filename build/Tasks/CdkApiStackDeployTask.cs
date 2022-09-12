using Cake.Common.Diagnostics;
using Cake.Frosting;
using CodingChallenge.Infrastructure.Extensions;

namespace Build.Tasks;

[TaskName("Cdk-ApiStack-Deploy-Task")]
public sealed class CdkApiStackDeployTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var logPrefix = "CDK API Stack";
        foreach (var projectSetting in context.Config.ProjectSettingsList)
            if (projectSetting.RunCdkDeploy)
            {
                context.Information($"{logPrefix}start cdk deployment for {projectSetting.ProjectName}");
                var mainStack = context.Config.AwsApplication.GetResourceName("apistack");
                context.DeployStack(projectSetting, mainStack);
            }
            else
            {
                context.Warning($"{logPrefix} deployment is not enabled for this project {projectSetting.ProjectName}");
            }
    }
}