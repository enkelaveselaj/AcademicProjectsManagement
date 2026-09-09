using AcademicProjects.Application.Common.Authorization;
using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed class GetNotificationsQueryHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUser)
    : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Notifications.AsNoTracking();

        if (!currentUser.IsAdministrator())
        {
            var userId = currentUser.GetUserId();
            query = query.Where(n => n.UserId == userId);
        }

        return await query
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
