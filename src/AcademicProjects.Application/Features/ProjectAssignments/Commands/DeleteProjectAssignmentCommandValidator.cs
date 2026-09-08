using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectAssignments.Commands;

public sealed class DeleteProjectAssignmentCommandValidator
    : AbstractValidator<DeleteProjectAssignmentCommand>
{
    public DeleteProjectAssignmentCommandValidator()
    {
        RuleFor(assignment => assignment.Id)
            .NotEmpty();
    }
}
