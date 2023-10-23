using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Core;

// TODO: There should be a better divide between logging and how it is displayed in the console
public class PhotoCopier
{
    private readonly IMediaTakenAtExtractor _mediaTakenAtExtractor;
    private readonly ILogger<PhotoCopier> _logger;

    public PhotoCopier(
        IMediaTakenAtExtractor mediaTakenAtExtractor,
        ILogger<PhotoCopier> logger)
    {
        _mediaTakenAtExtractor = mediaTakenAtExtractor;
        _logger = logger;
    }
    
    public void CopyPhotos(string sourceDirectory, string destinationDirectory, bool dryRun = false)
    {
        if (dryRun)
        {
            _logger.LogInformation("Performing a dry run");
            destinationDirectory = Path.Combine(destinationDirectory, "dry_run");
        }
        
        // TODO: Can we log to a spectre console sink?
        _logger.LogInformation(
            "Copying photos from {SourceDirectory} to {DestinationDirectory}",
            sourceDirectory,
            destinationDirectory);
        
        var photos = GetFiles(sourceDirectory);

        // TODO: Log how much is in each group ahead of time
        foreach (var (key, files) in photos)
        {
            var (year, month) = key;
            
            var destination = Path.Combine(
                destinationDirectory,
                year.ToString(CultureInfo.InvariantCulture),
                // Months are serialized with leading zeros
                month.ToString("00", CultureInfo.InvariantCulture));
            
            _logger.LogInformation(
                "Copying {FileCount} files to {DestinationDirectory}",
                files.Count,
                destination);
            
            Directory.CreateDirectory(destination);

            foreach (var file in files)
            {
                var destinationFile = Path.Combine(destination, Path.GetFileName(file));
                
                if (dryRun)
                {
                    File.Create(destinationFile);
                    
                    continue;
                }
                
                File.Copy(file, destinationFile);
            }
        }
        
        _logger.LogInformation(
            "Finished copying {Count} photos from {SourceDirectory} to {DestinationDirectory}",
            photos.Sum(group => group.Value.Count),
            sourceDirectory,
            destinationDirectory);
    }
    
    // Enumerate files ordered by creation date and grouped by year / month
    private IReadOnlyDictionary<(int Year, int Month), IReadOnlyList<string>> GetFiles(string directory)
        => Directory
            .EnumerateFiles(directory)
            .Select(file => (file, CreationTimeUtc: _mediaTakenAtExtractor.ExtractTakenAt(file).Value.ToDateTimeUtc()))
            .OrderBy(tuple => tuple.CreationTimeUtc)
            .GroupBy(tuple => (tuple.CreationTimeUtc.Year, tuple.CreationTimeUtc.Month))
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(tuple => tuple.file).ToList());
}
