using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class DeleteNotificationCommandHandler
    : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(
        DeleteNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == request.Id,
                cancellationToken);

        if (notification is null)
            return false;

        _context.Notifications.Remove(notification);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}