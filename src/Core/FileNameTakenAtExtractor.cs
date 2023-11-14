using System.Globalization;
using System.Text.RegularExpressions;
using FluentResults;
using NodaTime;
using NodaTime.Text;

namespace Core;

public partial class FileNameTakenAtExtractor : IMediaTakenAtExtractor
{
    [GeneratedRegex(@"\d{8}_\d{6}")]
    private static partial Regex DateTimePatternInFileName();
    
    public Result<Instant> ExtractTakenAt(string file)
    {
        var fileName = Path.GetFileNameWithoutExtension(file);

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

        var instant = result
            .Value
            .InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"])
            .ToInstant();

        return Result.Ok(instant);
    }
}
