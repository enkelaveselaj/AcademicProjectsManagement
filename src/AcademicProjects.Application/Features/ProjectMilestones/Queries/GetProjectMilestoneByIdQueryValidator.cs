using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectMilestones.Queries;

public sealed class GetProjectMilestoneByIdQueryValidator
    : AbstractValidator<GetProjectMilestoneByIdQuery>
{
    public GetProjectMilestoneByIdQueryValidator()
    {
        RuleFor(milestone => milestone.Id)
            .NotEmpty();
    }
}
