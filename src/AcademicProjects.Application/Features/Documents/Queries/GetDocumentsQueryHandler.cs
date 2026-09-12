using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetDocumentsQuery, IReadOnlyList<DocumentDto>>
{
    public async Task<IReadOnlyList<DocumentDto>> Handle(
        GetDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Documents.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var accessibleProjectIds = await projectAccess.GetAccessibleProjectIdsAsync(
                currentUser.GetUserId(),
                cancellationToken);

            query = query.Where(document => accessibleProjectIds.Contains(document.ProjectId));
        }

        return await query
            .Select(document => new DocumentDto(
                document.Id,
                document.FileName,
                document.ContentType,
                document.FileSizeBytes,
                document.UploadedById,
                document.ProjectId,
                document.CreatedAt,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
