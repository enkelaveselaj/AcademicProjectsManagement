using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class UpdateProjectAssignmentCommandValidator
    : AbstractValidator<UpdateProjectAssignmentCommand>
{
    public UpdateProjectAssignmentCommandValidator()
    {
        RuleFor(assignment => assignment.Id)
            .NotEmpty();

        RuleFor(assignment => assignment.ProjectId)
            .NotEmpty();

        RuleFor(assignment => assignment.UserId)
            .NotEmpty();

        RuleFor(assignment => assignment.Role)
            .NotEmpty()
            .MaximumLength(50);
    }
}
