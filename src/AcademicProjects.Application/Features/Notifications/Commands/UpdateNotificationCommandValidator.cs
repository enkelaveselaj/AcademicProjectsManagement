using FluentValidation;

namespace AcademicProjects.Application.Features.Notifications.Commands;

public sealed class UpdateNotificationCommandValidator
    : AbstractValidator<UpdateNotificationCommand>
{
    public UpdateNotificationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}