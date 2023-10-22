using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Spectre.Console;
using Spectre.Console.Cli;

#pragma warning disable CS8765

namespace Analysis;

public class AnalysisCommand : Command<AnalysisCommandSettings>
{
    public override int Execute(CommandContext context, AnalysisCommandSettings settings)
    {
        AnsiConsole.MarkupLine($"[bold]Checking Path: {settings.Path}[/]");
        
        // enumerate all the file infos in the directory
        var files = new DirectoryInfo(settings.Path)
            .EnumerateFiles()
            .ToArray();
        
        AnsiConsole.MarkupLine($"[bold]Found {files.Length} files[/]");
        
        // check how many files have exif date time
        var filesWithExifDateTime = files
            .Where(HasExifDateTime)
            .ToArray();
        
        AnsiConsole.MarkupLine($"[bold]Found {filesWithExifDateTime.Length} files with exif date time[/]");

        return 0;
    }
    
    private static bool HasExifDateTime(FileInfo fileInfo)
    {
        try
        {
            var metadata = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
            
            var subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            
            if (subIfdDirectory is null)
            {
                return false;
            }
            
            return subIfdDirectory.ContainsTag(ExifDirectoryBase.TagDateTimeOriginal);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
