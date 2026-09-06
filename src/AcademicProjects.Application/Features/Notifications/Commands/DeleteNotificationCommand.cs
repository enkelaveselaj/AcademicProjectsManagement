using MediatR;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed record DeleteNotificationCommand(Guid Id) : IRequest<bool>;