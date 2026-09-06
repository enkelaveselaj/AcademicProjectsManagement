using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using AcademicProjects.Domain.Entities;
using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class CreateNotificationCommandHandler
    : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    private readonly IApplicationDbContext _context;

    public CreateNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> Handle(
        CreateNotificationCommand request,
        CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Message = request.Message,
            Type = request.Type,
            IsRead = request.IsRead,
            UserId = request.UserId
        };

        _context.Notifications.Add(notification);

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