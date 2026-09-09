using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class DeleteDocumentCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectAccessService projectAccess,
    ProjectNotificationService notifier)
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

        var projectTitle = await context.Projects
            .Where(project => project.Id == document.ProjectId)
            .Select(project => project.Title)
            .FirstOrDefaultAsync(cancellationToken);

        await notifier.NotifyMembersAsync(
            document.ProjectId,
            currentUser.GetUserId(),
            $"Document removed from project '{projectTitle}': {document.FileName}.",
            NotificationType.Information,
            cancellationToken);

        context.Documents.Remove(document);

        await context.SaveChangesAsync(cancellationToken);
    }
}
