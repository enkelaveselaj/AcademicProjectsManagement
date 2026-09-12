using AcademicProjects.Domain.Enums;
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

        RuleFor(milestone => milestone.Description)
            .MaximumLength(1000);

        RuleFor(milestone => milestone.Status)
            .IsInEnum()
            .NotEqual(MilestoneStatus.Overdue)
            .WithMessage("Status must be Pending, InProgress, or Completed — Overdue is computed automatically.");

        RuleFor(milestone => milestone.ProjectId)
            .NotEmpty();
    }
}
