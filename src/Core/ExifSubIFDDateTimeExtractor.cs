using System.Globalization;
using FluentResults;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using NodaTime;
using NodaTime.Text;

namespace Core;

public class ExifSubIfdDateTimeExtractor : IMediaTakenAtExtractor
{
    public Result<Instant> ExtractTakenAt(string file)
    {
        var exifSubIfdDirectory = ImageMetadataReader.ReadMetadata(file)
            .OfType<ExifSubIfdDirectory>()
            .FirstOrDefault();

        if (exifSubIfdDirectory is null)
        {
            return Result.Fail($"Could not find ExifSubIfdDirectory in {file}");
        }
        
        // We get the string behind the Date/Time original tag and parse it as a LocalDateTime
        var dateTimeString = exifSubIfdDirectory.GetString(ExifDirectoryBase.TagDateTimeOriginal);
        
        if (dateTimeString is null)
        {
            return Result.Fail($"Could not find DateTimeOriginal in {file}");
        }

        var localDateTime = LocalDateTimePattern
            .Create("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture)
            .Parse(dateTimeString);
        
        if (!localDateTime.Success)
        {
            return Result.Fail($"Could not parse DateTimeOriginal in {file}");
        }
        
        // There sometimes isn't a time zone string, in which case we interpret the local time in Europe/Berlin
        var timeZoneString = exifSubIfdDirectory.GetString(ExifDirectoryBase.TagTimeZoneOriginal);

        if (timeZoneString is null)
        {
            var zonedDateTime = localDateTime.Value.InZoneLeniently(DateTimeZoneProviders.Tzdb["Europe/Berlin"]);
            return Result.Ok(zonedDateTime.ToInstant());
        }
        
        var offset = OffsetPattern.GeneralInvariant.Parse(timeZoneString);
        
        if (!offset.Success)
        {
            return Result.Fail($"Could not parse TimeZoneOriginal in {file}");
        }

        var offsetDateTime = localDateTime.Value.WithOffset(offset.Value);
        
        return Result.Ok(offsetDateTime.ToInstant());
    }
}
