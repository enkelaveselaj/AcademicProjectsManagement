using FluentValidation;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ResetUserPasswordCommandValidator : AbstractValidator<ResetUserPasswordCommand>
{
    public ResetUserPasswordCommandValidator()
    {
        RuleFor(command => command.NewPassword)
            .NotEmpty();
    }
}
