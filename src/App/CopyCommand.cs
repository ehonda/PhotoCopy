using Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace App;

[UsedImplicitly]
public class CopyCommand : Command<CopyCommandSettings>
{
    public override int Execute(CommandContext context, CopyCommandSettings settings)
    {
        // TODO: Use an actual logger
        var photoCopier = new PhotoCopier(new NullLogger<PhotoCopier>());

        try
        {
            photoCopier.CopyPhotos(settings.Source, settings.Target);
        }
        catch (Exception exception)
        {
            AnsiConsole.WriteException(exception);
            return 1;
        }

        return 0;
    }
}
