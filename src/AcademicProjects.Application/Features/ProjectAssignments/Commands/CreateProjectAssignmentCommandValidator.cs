using AcademicProjects.Domain.Enums;
using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class CreateProjectAssignmentCommandValidator
    : AbstractValidator<CreateProjectAssignmentCommand>
{
    public CreateProjectAssignmentCommandValidator()
    {
        RuleFor(assignment => assignment.ProjectId)
            .NotEmpty();

        RuleFor(assignment => assignment.UserId)
            .NotEmpty();

        RuleFor(assignment => assignment.Role)
            .NotEmpty()
            .MaximumLength(50)
            .Must(role => role.Trim() is nameof(UserRole.Student) or nameof(UserRole.Mentor))
            .WithMessage("Role must be Student or Mentor.");
    }
}
