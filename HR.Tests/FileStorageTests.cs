using HR.Services.Files;

namespace HR.Tests;

public class FileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "hr-storage-" + Guid.NewGuid().ToString("N"));
    private readonly FileStorageService _storage;

    public FileStorageTests() => _storage = new FileStorageService(_root, maxImageBytes: 32, maxDocumentBytes: 64);

    [Fact]
    public void Stage_CopiesAnImageUnderStaging_AndRemembersTheOriginalName()
    {
        var source = WriteTemp("photo.png", "png-bytes");

        var stored = _storage.Stage(source, FileKind.Image);

        Assert.StartsWith("_staging/", stored.RelativePath);
        Assert.Equal("photo.png", stored.OriginalFileName);
        Assert.Equal("image/png", stored.ContentType);
        Assert.Equal(FileStorageService.ComputeHash(_storage.FullPath(stored.RelativePath)), stored.Sha256);
        Assert.True(File.Exists(_storage.FullPath(stored.RelativePath)));
    }

    [Fact]
    public void Stage_RejectsAnExecutable_AndAnOversizedFile()
    {
        var exe = WriteTemp("run.exe", "nope");
        var big = WriteTemp("scan.pdf", new string('a', 80));

        Assert.Throws<InvalidOperationException>(() => _storage.Stage(exe, FileKind.Document));
        Assert.Throws<InvalidOperationException>(() => _storage.Stage(big, FileKind.Document));
    }

    [Fact]
    public void Promote_MovesTheFileUnderTheOwner_AndCleanupRemovesOrphans()
    {
        var source = WriteTemp("logo.png", "logo");
        var staged = _storage.Stage(source, FileKind.Image);

        var promoted = _storage.Promote(staged.RelativePath, "departments/4");

        Assert.StartsWith("departments/4/", promoted.RelativePath);
        Assert.False(File.Exists(_storage.FullPath(staged.RelativePath)));
        Assert.Equal(promoted.Sha256, FileStorageService.ComputeHash(_storage.FullPath(promoted.RelativePath)));

        File.WriteAllText(Path.Combine(_root, "orphan.txt"), "stray");
        var removed = _storage.DeleteUnreferenced(new[] { promoted.RelativePath });

        Assert.Equal(1, removed);
        Assert.False(File.Exists(Path.Combine(_root, "orphan.txt")));
        Assert.True(File.Exists(_storage.FullPath(promoted.RelativePath)));
    }

    [Fact]
    public void MatchingPath_ReusesTheExistingFileForTheSameHash()
    {
        var match = FileStorageService.MatchingPath(
            new[]
            {
                ("abc", "employees/1/old.png", true),
                ("abc", "employees/1/keep.png", false)
            },
            "ABC");

        Assert.Equal("employees/1/keep.png", match);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private static string WriteTemp(string name, string contents)
    {
        var folder = Path.Combine(Path.GetTempPath(), "hr-src-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, name);
        File.WriteAllText(path, contents);
        return path;
    }
}
