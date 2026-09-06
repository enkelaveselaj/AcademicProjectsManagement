using FluentValidation;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class CreateNotificationCommandValidator
    : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}