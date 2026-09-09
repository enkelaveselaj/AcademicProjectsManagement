using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class DeleteDocumentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess)
    : IRequestHandler<DeleteDocumentCommand>
{
    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var document = await context.Documents
            .FirstOrDefaultAsync(
                document => document.Id == request.Id,
                cancellationToken);

        if (document is null)
        {
            throw new NotFoundException("Document", request.Id);
        }

        if (!currentUser.IsAdministrator()
            && !await projectAccess.IsMemberAsync(document.ProjectId, currentUser.GetUserId(), cancellationToken))
        {
            throw new ForbiddenAccessException("Only a project member or an administrator can delete this document.");
        }

        context.Documents.Remove(document);

        await context.SaveChangesAsync(cancellationToken);
    }
}