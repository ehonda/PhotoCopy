using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace App;

[UsedImplicitly]
public class CopyCommand : Command<CopyCommandSettings>
{
    public override int Execute(CommandContext context, CopyCommandSettings settings)
    {
        // TODO: Use the actual PhotoCopier
        AnsiConsole.MarkupLine("Copying photos from [green]{0}[/] to [green]{1}[/]", settings.Source, settings.Target);
        
        return 0;
    }
}
