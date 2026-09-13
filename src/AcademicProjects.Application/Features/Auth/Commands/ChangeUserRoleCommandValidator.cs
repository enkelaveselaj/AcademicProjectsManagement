using FluentValidation;

namespace AcademicProjects.Application.Features.Auth.Commands;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(command => command.Role)
            .NotEmpty();
    }
}
