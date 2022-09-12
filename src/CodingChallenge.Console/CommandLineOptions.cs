using CommandLine;

namespace CodingChallenge.Console;

public class CommandLineOptions
{
    [Option('r', "reset", Required = false, HelpText = "Deletes all data previously processed by the program.")]
    public bool Reset { get; set; }
}
