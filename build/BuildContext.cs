using Build.Models;
using Cake.Core;
using Cake.Frosting;

namespace Build;

public partial class BuildContext : FrostingContext
{
    public ICakeContext _context { get; set; }
    public Settings Config { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        _context = context;
        Config = new Settings(context);
    }

    public string GetProjectFilePathUsingBuiltinArguments(string projectName)
    {
        return $"{this.Config.StandardFolders.SourceFullPath}/{projectName}/{projectName}.csproj";
    }
}
