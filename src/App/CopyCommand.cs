using Core;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

#pragma warning disable CS8765

namespace App;

[UsedImplicitly]
public class CopyCommand : Command<CopyCommandSettings>
{
    public override int Execute(CommandContext context, CopyCommandSettings settings)
    {
        var extractor = new FallbackExtractor(
            new ExifSubIfdDateTimeExtractor(),
            new FallbackExtractor(
                new QuickTimeMovieHeaderExtractor(),
                new FileCreationTimeExtractor()));

        // See: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#non-host-console-app
        // TODO: Use Serilog sink? https://github.com/serilog/serilog-sinks-console
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.UseUtcTimestamp = true;
                })
                .SetMinimumLevel(LogLevel.Debug);
        });

        var logger = loggerFactory.CreateLogger<PhotoCopier>();

        var photoCopier = new PhotoCopier(extractor, logger);

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
