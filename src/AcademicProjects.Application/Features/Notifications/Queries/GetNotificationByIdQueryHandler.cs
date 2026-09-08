using AcademicProjects.Application.Common.Exceptions;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed class GetNotificationByIdQueryHandler
    : IRequestHandler<GetNotificationByIdQuery, NotificationDto>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationDto> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.Id == request.Id)
            .Select(n => new NotificationDto(
                n.Id,
                n.Message,
                n.Type,
                n.IsRead,
                n.UserId,
                n.CreatedAt,
                n.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return notification ?? throw new NotFoundException("Notification", request.Id);
    }
}
