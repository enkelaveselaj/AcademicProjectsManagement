using FluentValidation;

namespace AcademicProjects.Application.Features.Projects.Commands;

public sealed class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(4000);

        RuleFor(command => command.Status)
            .IsInEnum();

        RuleFor(command => command.CategoryId)
            .NotEmpty();
    }
}