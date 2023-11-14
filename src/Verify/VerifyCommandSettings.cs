using System.ComponentModel;
using Spectre.Console.Cli;

namespace Verify;

public class VerifyCommandSettings : CommandSettings
{
    [Description("The path directory.")]
    [CommandArgument(0, "<PATH>")]
    public string Path { get; init; } = null!;
}
