using System.ComponentModel;
using Build.Models;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;

namespace Build;
public partial class BuildContext
{
    
    public string GetCdkParamOverrides()
    {        
        var retString = "";
        
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(this.Config.AwsApplication))
        {
            string name = descriptor!.Name!;
            object value = descriptor.GetValue(Config.AwsApplication)!;
            retString += $"-c \"{name}={value}\" ";
        }
        return retString;
    }
    public void DeployStack(ProjectSettings projectSetting, string stackname)
    {
        var stackFullPath = System.IO.Path.Combine(this.Config.StandardFolders.RootFullPath, $"src/{projectSetting.ProjectName}/bin/{Config.DotnetSettings.DotnetConfiguration}/{Config.DotnetSettings.DotnetFramework}/{projectSetting.ProjectName}.dll");
        this.Information($"Stack Full Path is {stackFullPath}");
        var cdkAppPath = $"dotnet {stackFullPath}";
        _context.Information($"cdk app path is {cdkAppPath}");

        var parameterOverrides = GetCdkParamOverrides()!;
        _context.Information($"param over {parameterOverrides} ... stackname : {stackname}");

        var arguments = new ProcessArgumentBuilder()
                    .Append("deploy")
                    .Append("--require-approval=never")
                    .Append("--verbose")
                    .Append("-o ../.cdk")
                    .Append($"--app \"{cdkAppPath}\"")
                    .Append($"{stackname}")
                    .Append($"{parameterOverrides}");
        _ =
        _context.StartProcess(
            "cdk",
            new ProcessSettings
            {
                Arguments = arguments,
                RedirectStandardOutput = true
            },
            out _
        );

        _context.Information($"output -- cdk {arguments.Render()}");
    }
}
