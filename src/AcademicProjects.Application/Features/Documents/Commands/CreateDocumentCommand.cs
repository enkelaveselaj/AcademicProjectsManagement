using AcademicProjects.Application.Features.Documents.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed record CreateDocumentCommand(
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Stream Content,
    Guid ProjectId) : IRequest<DocumentDto>;
