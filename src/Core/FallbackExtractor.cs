using FluentResults;
using NodaTime;

namespace Core;

public class FallbackExtractor : IMediaTakenAtExtractor
{
    private readonly IMediaTakenAtExtractor _primaryExtractor;
    private readonly IMediaTakenAtExtractor _secondaryExtractor;

    public FallbackExtractor(IMediaTakenAtExtractor primaryExtractor, IMediaTakenAtExtractor secondaryExtractor)
    {
        _primaryExtractor = primaryExtractor;
        _secondaryExtractor = secondaryExtractor;
    }

    public Result<Instant> ExtractTakenAt(string file)
    {
        var primaryResult = _primaryExtractor.ExtractTakenAt(file);

        return primaryResult.IsSuccess ? primaryResult : _secondaryExtractor.ExtractTakenAt(file);
    }
}
