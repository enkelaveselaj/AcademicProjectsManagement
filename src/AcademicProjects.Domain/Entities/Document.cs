using AcademicProjects.Domain.Common;

namespace AcademicProjects.Domain.Entities;

public class Document : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Server-generated name the file is actually stored under. Never exposed to clients -
    /// downloads are always resolved through the document's Id, not this value, to avoid
    /// path traversal or leaking storage layout.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public Guid UploadedById { get; set; }

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;
}
