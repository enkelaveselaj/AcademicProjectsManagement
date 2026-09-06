using AcademicProjects.Domain.Enums;

namespace AcademicProjects.Application.Features.Notifications.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Message,
    NotificationType Type,
    bool IsRead,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);