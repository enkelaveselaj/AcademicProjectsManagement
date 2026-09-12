namespace AcademicProjects.Application.Features.Documents.DTOs;

public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Guid UploadedById,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
