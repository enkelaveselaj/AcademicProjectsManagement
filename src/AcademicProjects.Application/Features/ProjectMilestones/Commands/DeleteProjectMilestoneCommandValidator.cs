using FluentValidation;

namespace AcademicProjects.Application.Features.ProjectMilestones.Commands;

public sealed class DeleteProjectMilestoneCommandValidator
    : AbstractValidator<DeleteProjectMilestoneCommand>
{
    public DeleteProjectMilestoneCommandValidator()
    {
        RuleFor(milestone => milestone.Id)
            .NotEmpty();
    }
}
