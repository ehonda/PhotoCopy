using FluentResults;
using NodaTime;

namespace Core;

public interface IMediaTakenAtExtractor
{
    Result<Instant> ExtractTakenAt(string file);
}
