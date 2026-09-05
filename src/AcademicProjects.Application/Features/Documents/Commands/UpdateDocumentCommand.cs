using AcademicProjects.Application.Features.Documents.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed record UpdateDocumentCommand(
    Guid Id,
    string FileName,
    string FilePath,
    Guid ProjectId) : IRequest<DocumentDto?>;