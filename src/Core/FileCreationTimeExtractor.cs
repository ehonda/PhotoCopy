using FluentResults;
using NodaTime;
using NodaTime.Extensions;

namespace Core;

public class FileCreationTimeExtractor : IMediaTakenAtExtractor
{
    public Result<Instant> ExtractTakenAt(string file)
        => new FileInfo(file).CreationTimeUtc.ToInstant();
}
