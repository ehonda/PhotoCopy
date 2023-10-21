using System.ComponentModel;
using JetBrains.Annotations;
using Spectre.Console.Cli;

namespace App;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class CopyCommandSettings : CommandSettings
{
    [Description("The source directory path.")]
    [CommandArgument(0, "<SOURCE>")]
    public string Source { get; init; } = null!;
    
    [Description("The target directory path.")]
    [CommandArgument(1, "<TARGET>")]
    public string Target { get; init; } = null!;
}
