using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class CreateProjectMilestoneCommandValidator
    : AbstractValidator<CreateProjectMilestoneCommand>
{
    public CreateProjectMilestoneCommandValidator()
    {
        RuleFor(milestone => milestone.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(milestone => milestone.Description)
            .MaximumLength(1000);

        RuleFor(milestone => milestone.ProjectId)
            .NotEmpty();
    }
}
