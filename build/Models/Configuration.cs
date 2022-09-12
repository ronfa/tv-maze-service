using System.ComponentModel;
using AutoMapper;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.Yaml;
using CodingChallenge.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Build.Models;

public class Settings
{
    public AWSAppProject AwsApplication;

    public Settings(ICakeContext cakeContext)
    {
        ProjectSettingsList = new List<ProjectSettings>();
        metadata = cakeContext.DeserializeYamlFromFile<MetaData>(new FilePath("../metadata.yaml"));
        AwsApplication = new AWSAppProject();
        SetAwsAppProject(cakeContext);
        StandardFolders = new StandardFolderSettings(cakeContext);
        DotnetSettings = new DotnetSettings(cakeContext);
    }

    public StandardFolderSettings StandardFolders { get; }
    public DotnetSettings DotnetSettings { get; }
    public List<ProjectSettings> ProjectSettingsList { get; }

    public MetaData metadata { get; }

    private void SetMetadataProperties(ICakeContext cakeContext)
    {
    }

    private void PrintProperties(ICakeContext cakeContext, object obj)
    {
        foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
        {
            var name = descriptor.Name;
            var value = descriptor.GetValue(obj)!;
            cakeContext.Information($"{obj.GetType().Name} - Property Name {name} - Value: {value}");
        }
    }

    private void SetAwsAppProject(ICakeContext cakeContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables().Build();
        configuration.GetSection(Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX).Bind(AwsApplication);

        var autoMapperConfig = new MapperConfiguration(
            cfg => cfg.CreateMap<MetaData, AWSAppProject>().ForAllMembers(opt =>
                opt.Condition((src, dest, sourceMember) => !string.IsNullOrWhiteSpace((string) sourceMember)))
        );
        var autoMapper = autoMapperConfig.CreateMapper();
        autoMapper.Map(metadata, AwsApplication);
        cakeContext.Information($"metadata platform is {metadata.Platform}");
        cakeContext.Information($"AwsApplication platform is {AwsApplication.Platform}");
        cakeContext.Information($"json object {JsonConvert.SerializeObject(AwsApplication)}");
        Environment.SetEnvironmentVariable(
            $"{Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX}__{nameof(AwsApplication.Platform)}",
            AwsApplication.Platform);
        Environment.SetEnvironmentVariable(
            $"{Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX}__{nameof(AwsApplication.System)}", AwsApplication.System);
        Environment.SetEnvironmentVariable(
            $"{Constants.APPLICATION_ENVIRONMENT_VAR_PREFIX}__{nameof(AwsApplication.Subsystem)}",
            AwsApplication.Subsystem);
    }
}