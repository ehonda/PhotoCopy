using System.ComponentModel;
using Spectre.Console.Cli;

namespace Analysis;

public class AnalysisCommandSettings : CommandSettings
{
    [Description("The path directory.")]
    [CommandArgument(0, "<PATH>")]
    public string Path { get; init; } = null!;
}
