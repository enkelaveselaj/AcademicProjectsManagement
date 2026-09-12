using MediatR;

namespace AcademicProjects.Application.Features.Documents.Queries;

/// <summary>
/// Server-internal lookup used by the download endpoint. Unlike <see cref="GetDocumentByIdQuery"/>,
/// this exposes StoredFileName - never something a public DTO should carry - so it stays a
/// separate query rather than a field added to DocumentDto.
/// </summary>
public sealed record GetDocumentFileQuery(Guid Id) : IRequest<DocumentFileDto>;

public sealed record DocumentFileDto(string FileName, string ContentType, string StoredFileName);
