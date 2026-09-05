using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentByIdQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetDocumentByIdQuery, DocumentDto?>
{
    public async Task<DocumentDto?> Handle(
        GetDocumentByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Documents
            .AsNoTracking()
            .Where(document => document.Id == request.Id)
            .Select(document => new DocumentDto(
                document.Id,
                document.FileName,
                document.FilePath,
                document.ProjectId,
                document.CreatedAt,
                document.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}