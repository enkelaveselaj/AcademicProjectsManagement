using AcademicProjects.Application.Features.Documents.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed record GetDocumentByIdQuery(Guid Id) : IRequest<DocumentDto?>;