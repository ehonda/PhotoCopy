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
        
        // We get the string behind the Date/Time original tag, together with the Time Zone Original tag
        // and parse it to a ZonedDateTime
        var dateTimeString = exifSubIfdDirectory.GetString(ExifDirectoryBase.TagDateTimeOriginal);
        var timeZoneString = exifSubIfdDirectory.GetString(ExifDirectoryBase.TagTimeZoneOriginal);
        
        if (dateTimeString is null || timeZoneString is null)
        {
            return Result.Fail($"Could not find DateTimeOriginal or TimeZoneOriginal in {file}");
        }

        var localDateTime = LocalDateTimePattern
            .Create("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture)
            .Parse(dateTimeString);
        
        if (!localDateTime.Success)
        {
            return Result.Fail($"Could not parse DateTimeOriginal in {file}");
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
