using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class UpdateProjectMilestoneCommandValidator
    : AbstractValidator<UpdateProjectMilestoneCommand>
{
    public UpdateProjectMilestoneCommandValidator()
    {
        RuleFor(milestone => milestone.Id)
            .NotEmpty();

        RuleFor(milestone => milestone.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(milestone => milestone.ProjectId)
            .NotEmpty();
    }
}
