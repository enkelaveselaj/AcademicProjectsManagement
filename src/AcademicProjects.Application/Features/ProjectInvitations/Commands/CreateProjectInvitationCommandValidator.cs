using AcademicProjects.Domain.Enums;
using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectInvitations.Commands;

public sealed class CreateProjectInvitationCommandValidator
    : AbstractValidator<CreateProjectInvitationCommand>
{
    public CreateProjectInvitationCommandValidator()
    {
        RuleFor(invitation => invitation.ProjectId)
            .NotEmpty();

        RuleFor(invitation => invitation.InvitedUserId)
            .NotEmpty();

        RuleFor(invitation => invitation.Role)
            .NotEmpty()
            .MaximumLength(50)
            .Must(role => role.Trim() is nameof(UserRole.Student) or nameof(UserRole.Mentor))
            .WithMessage("Role must be Student or Mentor.");
    }
}
