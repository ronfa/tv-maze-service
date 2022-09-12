using Cake.Common.Diagnostics;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Core;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("DotnetCore-Build-Task")]
public sealed class DotnetCoreBuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var msBuildSettings = new DotNetCoreMSBuildSettings
        {
            Verbosity = context.Config.DotnetSettings.DotnetCoreVerbosity
        };

        var settings = new DotNetBuildSettings
        {
            Configuration = context.Config.DotnetSettings.DotnetConfiguration,
            MSBuildSettings = msBuildSettings,
            NoRestore = true,
            NoLogo = true,
            NoIncremental = true,
            ArgumentCustomization = args =>
                args.Append(
                    $"/nodeReuse:false -p:Version={context.Config.AwsApplication.Version} -p:UseSharedCompilation=false -maxcpucount:1")
        };

        if (!string.IsNullOrEmpty(context.Config.DotnetSettings.DotnetFramework))
        {
            context.Information($"dotnet build - framework set as {context.Config.DotnetSettings.DotnetFramework}.");
            settings.Framework = context.Config.DotnetSettings.DotnetFramework;
        }
        else
        {
            context.Information("dotnet build  - Using default framework.");
        }

        foreach (var solution in context.Config.StandardFolders.Solutions)
        {
            context.Information("Building '{0}'...", solution);
            context.DotNetBuild(solution.FullPath, settings);
            context.Information("'{0}' has been built.", solution);
            context.DotNetBuildServerShutdown();
        }
    }
}