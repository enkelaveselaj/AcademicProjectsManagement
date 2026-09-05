using FluentValidation;

namespace AcademicProjects.Application.Features.Comments.Commands;

public sealed class UpdateCommentCommandValidator
    : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(comment => comment.Id)
            .NotEmpty();

        RuleFor(comment => comment.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(comment => comment.ProjectId)
            .NotEmpty();
    }
}