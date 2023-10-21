using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime.Text;

namespace Core.Tests;

[TestOf(typeof(PhotoCopier))]
public class PhotoCopierTests
{
    private static class Sandbox
    {
        private const string SandboxDirectory = "photo_copier_sandbox";
        private const string SourceDirectory = "source";
        private const string DestinationDirectory = "destination";
        
        public static void Create()
        {
            Directory.CreateDirectory(SandboxDirectory);
            Directory.CreateDirectory(Path.Combine(SandboxDirectory, SourceDirectory));
            Directory.CreateDirectory(Path.Combine(SandboxDirectory, DestinationDirectory));
        }
        
        public static void Delete()
        {
            if (Directory.Exists(SandboxDirectory))
            {
                Directory.Delete(SandboxDirectory, true);
            }
        }
        
        public static string GetSourceDirectory()
            => Path.Combine(SandboxDirectory, SourceDirectory);
        
        public static string GetDestinationDirectory()
            => Path.Combine(SandboxDirectory, DestinationDirectory);
        
        // Get source directory file names
        public static IEnumerable<string> GetSourceFiles()
            => Directory
                .EnumerateFiles(GetSourceDirectory())
                .Select(Path.GetFileName)!;
        
        // Get destination subdirectories
        public static IEnumerable<string> GetDestinationSubdirectories()
            => Directory
                .EnumerateDirectories(GetDestinationDirectory())
                .Select(Path.GetFileName)!;
        
        // Get subdirectories in directory
        public static IEnumerable<string> GetSubdirectories(string directory)
            => Directory
                .EnumerateDirectories(directory)
                .Select(Path.GetFileName)!;
        
        // Get files in directory
        public static IEnumerable<string> GetFiles(string directory)
            => Directory
                .EnumerateFiles(directory)
                .Select(Path.GetFileName)!;
        
        public static void InsertFile(string fileName, string creationTimeUtc)
            => InsertFile(
                fileName,
                InstantPattern
                    .Create("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    .Parse(creationTimeUtc)
                    .Value
                    .ToDateTimeUtc());
        
        private static void InsertFile(string fileName, DateTime creationTimeUtc)
        {
            var filePath = Path.Combine(SandboxDirectory, SourceDirectory, fileName);
            File.WriteAllText(filePath, string.Empty);
            File.SetCreationTimeUtc(filePath, creationTimeUtc);
        }
    }
    
    [SetUp]
    public void SetUp()
    {
        Sandbox.Delete();
        Sandbox.Create();
    }
    
    [TearDown]
    public void TearDown()
    {
        Sandbox.Delete();
    }

    // TODO: Improve test
    [Test]
    public void copying_works()
    {
        // Arrange
        const string januaryFileA = "january_file_a.jpg";
        const string januaryFileB = "january_file_b.jpg";
        const string decemberFileA = "december_file_a.jpg";
        
        Sandbox.InsertFile(januaryFileA, "2023-01-01 00:00:00");
        Sandbox.InsertFile(januaryFileB, "2023-01-02 00:10:00");
        Sandbox.InsertFile(decemberFileA, "2023-12-31 23:59:59");
        
        // Act
        var photoCopier = new PhotoCopier(new NullLogger<PhotoCopier>());
        photoCopier.CopyPhotos(Sandbox.GetSourceDirectory(), Sandbox.GetDestinationDirectory());

        // Assert
        var allFiles = new[] { januaryFileA, januaryFileB, decemberFileA };
        Sandbox.GetSourceFiles().Should().BeEquivalentTo(allFiles);
        
        var expectedDestinationSubdirectories = new[] { "2023" };
        Sandbox.GetDestinationSubdirectories().Should().BeEquivalentTo(expectedDestinationSubdirectories);
        
        var expectedSubdirectories = new[] { "01", "12" };
        Sandbox.GetSubdirectories(Path.Combine(Sandbox.GetDestinationDirectory(), "2023"))
            .Should().BeEquivalentTo(expectedSubdirectories);
        
        var expectedJanuaryFiles = new[] { januaryFileA, januaryFileB };
        Sandbox.GetFiles(Path.Combine(Sandbox.GetDestinationDirectory(), "2023", "01"))
            .Should().BeEquivalentTo(expectedJanuaryFiles);
        
        var expectedDecemberFiles = new[] { decemberFileA };
        Sandbox.GetFiles(Path.Combine(Sandbox.GetDestinationDirectory(), "2023", "12"))
            .Should().BeEquivalentTo(expectedDecemberFiles);
    }
}
