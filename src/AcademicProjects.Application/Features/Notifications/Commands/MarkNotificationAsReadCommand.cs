using AcademicProjects.Application.Features.Notifications.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed record MarkNotificationAsReadCommand(Guid Id) : IRequest<NotificationDto>;
