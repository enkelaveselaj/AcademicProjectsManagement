using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Documents.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentByIdQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
{
    public async Task<DocumentDto> Handle(
        GetDocumentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .AsNoTracking()
            .Where(document => document.Id == request.Id)
            .Select(document => new DocumentDto(
                document.Id,
                document.FileName,
                document.FilePath,
                document.UploadedById,
                document.ProjectId,
                document.CreatedAt,
                document.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Document", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(document.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("You do not have access to this document.");
        }

        return document;
    }
}
