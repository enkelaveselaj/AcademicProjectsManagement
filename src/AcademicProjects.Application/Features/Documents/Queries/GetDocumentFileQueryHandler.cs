using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Queries;

public sealed class GetDocumentFileQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<GetDocumentFileQuery, DocumentFileDto>
{
    public async Task<DocumentFileDto> Handle(
        GetDocumentFileQuery request,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .AsNoTracking()
            .Where(document => document.Id == request.Id)
            .Select(document => new
            {
                document.FileName,
                document.ContentType,
                document.StoredFileName,
                document.ProjectId
            })
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

        return new DocumentFileDto(document.FileName, document.ContentType, document.StoredFileName);
    }
}
