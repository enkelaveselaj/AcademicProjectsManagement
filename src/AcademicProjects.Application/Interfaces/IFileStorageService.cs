namespace AcademicProjects.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Persists the given content and returns the server-generated name it was stored under.
    /// </summary>
    Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
}
