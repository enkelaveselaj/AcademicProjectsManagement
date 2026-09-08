using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectAssignments.Queries;

public sealed class GetProjectAssignmentByIdQueryValidator
    : AbstractValidator<GetProjectAssignmentByIdQuery>
{
    public GetProjectAssignmentByIdQueryValidator()
    {
        RuleFor(assignment => assignment.Id)
            .NotEmpty();
    }
}
