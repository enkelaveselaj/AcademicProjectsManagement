using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Common.Notifications;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class DeleteProjectCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser,
    ProjectNotificationService notifier)
    : IRequestHandler<DeleteProjectCommand>
{
    public async Task Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await context.Projects
            .FirstOrDefaultAsync(
                project => project.Id == request.Id,
                cancellationToken);

        if (project is null)
        {
            throw new NotFoundException("Project", request.Id);
        }

        if (!currentUser.IsAdministrator() && project.CreatedById != currentUser.GetUserId())
        {
            throw new ForbiddenAccessException("Only the project's creator or an administrator can delete this project.");
        }

        await notifier.NotifyMembersAsync(
            project.Id,
            currentUser.GetUserId(),
            $"Project '{project.Title}' was deleted.",
            NotificationType.Warning,
            cancellationToken);

        context.Projects.Remove(project);

        await context.SaveChangesAsync(cancellationToken);
    }
}
