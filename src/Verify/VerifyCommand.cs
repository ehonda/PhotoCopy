using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using NodaTime;
using NodaTime.Text;
using Spectre.Console;
using Spectre.Console.Cli;

#pragma warning disable CS8765

namespace Verify;

public partial class VerifyCommand : Command<VerifyCommandSettings>
{
    [GeneratedRegex(@"\d{8}_\d{6}")]
    private static partial Regex DateTimePatternInFileName();
    
    public override int Execute(CommandContext context, VerifyCommandSettings settings)
    {
        AnsiConsole.MarkupLine($"[bold]Checking Path: {settings.Path}[/]");

        foreach (var year in Directory.EnumerateDirectories(settings.Path))
        {
            AnsiConsole.MarkupLine($"[bold]Checking Year: {year}[/]");
            
            foreach (var month in Directory.EnumerateDirectories(year))
            {
                AnsiConsole.MarkupLine($"[bold]Checking Month: {month}[/]");
                
                foreach (var file in Directory.EnumerateFiles(month))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);

                    var shouldBeCorrect = ShouldBeCorrect(year, month, fileName);
                    
                    if (!shouldBeCorrect)
                    {
                        AnsiConsole.MarkupLine($"[bold red]File {file} should probably be in {year}/{month}[/]");
                    }
                }
            }
        }

        return 0;
    }

    private static Result<ZonedDateTime> FileNameZonedDateTime(string fileName)
    {
        var dateTimeMatches = DateTimePatternInFileName().Match(fileName);
        
        if (!dateTimeMatches.Success)
        {
            return Result.Fail($"Could not find DateTime in {fileName}");
        }
        
        var dateTimeString = dateTimeMatches.Value;

        var result = LocalDateTimePattern
            .Create("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture)
            .Parse(dateTimeString);

        if (!result.Success)
        {
            return Result.Fail($"Could not parse {dateTimeString} as an Instant");
        }

        var zonedDateTime = result
            .Value
            .InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]);

        return Result.Ok(zonedDateTime);
    }

    private static bool ShouldBeCorrect(string yearFolderPath, string monthFolderPath, string fileName)
    {
        var year = Path.GetFileName(yearFolderPath);
        var month = Path.GetFileName(monthFolderPath);
        
        var zonedDateTime = FileNameZonedDateTime(fileName);
        
        if (zonedDateTime.IsFailed)
        {
            return false;
        }
        
        var yearMatches = zonedDateTime.Value.Year.ToString() == year;
        var monthMatches = zonedDateTime.Value.Month.ToString() == month;
        
        return yearMatches && monthMatches;
    }
}
