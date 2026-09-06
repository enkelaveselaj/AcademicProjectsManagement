using AcademicProjects.Application.Features.Notifications.DTOs;
using AcademicProjects.Domain.Enums;
using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed record UpdateNotificationCommand(
    Guid Id,
    string Message,
    NotificationType Type,
    bool IsRead,
    Guid UserId) : IRequest<NotificationDto?>;