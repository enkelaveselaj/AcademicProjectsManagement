using FluentValidation;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class DeleteNotificationCommandValidator
    : AbstractValidator<DeleteNotificationCommand>
{
    public DeleteNotificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}