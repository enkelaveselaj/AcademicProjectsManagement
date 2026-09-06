using AcademicProjects.Domain.Enums;

namespace AcademicProjects.API.Features.Notifications;

public sealed record UpdateNotificationRequest(
    string Message,
    NotificationType Type,
    bool IsRead,
    Guid UserId);