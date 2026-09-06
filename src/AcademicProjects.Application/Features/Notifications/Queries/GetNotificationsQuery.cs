using AcademicProjects.Application.Features.Notifications.DTOs;
using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Queries;

public sealed record GetNotificationsQuery
    : IRequest<List<NotificationDto>>;