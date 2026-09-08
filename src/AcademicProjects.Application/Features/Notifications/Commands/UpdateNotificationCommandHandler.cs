using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class UpdateNotificationCommandHandler
    : IRequestHandler<UpdateNotificationCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> Handle(
        UpdateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(
                n => n.Id == request.Id,
                cancellationToken);

        if (notification is null)
            throw new NotFoundException("Notification", request.Id);

        notification.Message = request.Message;
        notification.Type = request.Type;
        notification.IsRead = request.IsRead;
        notification.UserId = request.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationDto(
            notification.Id,
            notification.Message,
            notification.Type,
            notification.IsRead,
            notification.UserId,
            notification.CreatedAt,
            notification.UpdatedAt);
    }
}