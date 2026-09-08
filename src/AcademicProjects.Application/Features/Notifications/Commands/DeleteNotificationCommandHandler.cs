using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == request.Id,
                cancellationToken);

        if (notification is null)
            throw new NotFoundException("Notification", request.Id);

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
