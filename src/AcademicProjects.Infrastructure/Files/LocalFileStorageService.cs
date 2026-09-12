using AcademicProjects.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AcademicProjects.Infrastructure.Files;

/// <summary>
/// Stores uploaded files on local disk, outside wwwroot, so the only way to read one back is
/// through the authenticated download endpoint (which enforces the same project-membership
/// checks as every other document operation) rather than a directly web-servable static path.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "uploads");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileExtension,
        CancellationToken cancellationToken = default)
    {
        var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";
        var fullPath = Path.Combine(_rootPath, storedFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        Stream stream = File.OpenRead(GetFullPath(storedFileName));
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storedFileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string storedFileName)
    {
        // Guard against path traversal - storedFileName is always a bare name we generated
        // ourselves, but this keeps the guarantee even if that ever changes.
        var safeFileName = Path.GetFileName(storedFileName);
        return Path.Combine(_rootPath, safeFileName);
    }
}
