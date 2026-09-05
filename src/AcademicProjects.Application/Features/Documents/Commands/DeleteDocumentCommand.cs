using MediatR;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed record DeleteDocumentCommand(Guid Id) : IRequest<bool>;