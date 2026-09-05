namespace AcademicProjects.Application.Features.Documents.DTOs;

public sealed record DocumentDto(
    Guid Id,
    string FileName,
    string FilePath,
    Guid ProjectId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);