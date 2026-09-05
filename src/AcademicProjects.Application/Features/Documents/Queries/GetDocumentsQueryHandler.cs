using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    public async Task<IReadOnlyList<DocumentDto>> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        return await context.Documents
            .AsNoTracking()
            .Select(document => new DocumentDto(
                document.Id,
                document.FileName,
                document.FilePath,
                document.ProjectId,
                document.CreatedAt,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}