using FluentValidation;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty();

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
