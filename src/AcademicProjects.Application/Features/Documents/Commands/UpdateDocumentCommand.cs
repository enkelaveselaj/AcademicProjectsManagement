using AcademicProjects.Application.Features.Documents.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Documents.Commands;

/// <summary>
/// Renames a document or moves it to a different project. Re-uploading new bytes isn't
/// supported here - that's a delete-and-upload-again, since the stored file itself never
/// changes as part of an update.
/// </summary>
public sealed record UpdateDocumentCommand(
    Guid Id,
    string FileName,
    Guid ProjectId) : IRequest<DocumentDto>;
