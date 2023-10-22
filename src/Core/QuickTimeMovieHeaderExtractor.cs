using FluentResults;
using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using NodaTime;
using NodaTime.Extensions;

namespace Core;

public class QuickTimeMovieHeaderExtractor : IMediaTakenAtExtractor
{
    public Result<Instant> ExtractTakenAt(string file)
    {
        var quickTimeMovieHeaderDirectory = ImageMetadataReader.ReadMetadata(file)
            .OfType<QuickTimeMovieHeaderDirectory>()
            .FirstOrDefault();

        if (quickTimeMovieHeaderDirectory is null)
        {
            return Result.Fail($"Could not find QuickTimeMovieHeaderDirectory in {file}");
        }
        
        var creationTime = quickTimeMovieHeaderDirectory.GetDateTime(QuickTimeMovieHeaderDirectory.TagCreated);
        var creationTimeUtc = new DateTime(creationTime.Ticks, DateTimeKind.Utc);

        return Result.Ok(creationTimeUtc.ToInstant());
    }
}
