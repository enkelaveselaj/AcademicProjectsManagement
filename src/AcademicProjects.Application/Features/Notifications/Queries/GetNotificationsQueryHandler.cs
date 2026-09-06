using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Select(n => new NotificationDto(
                n.Id,
                n.Message,
                n.Type,
                n.IsRead,
                n.UserId,
                n.CreatedAt,
                n.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}