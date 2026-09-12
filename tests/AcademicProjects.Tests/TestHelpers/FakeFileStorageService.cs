using AcademicProjects.Application.Interfaces;

namespace AcademicProjects.Tests.TestHelpers;

public sealed class FakeFileStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);

        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";
        _files[storedFileName] = memoryStream.ToArray();

        return Task.FromResult(storedFileName);
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream>(new MemoryStream(_files[storedFileName]));
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        _files.Remove(storedFileName);
        return Task.CompletedTask;
    }
}
