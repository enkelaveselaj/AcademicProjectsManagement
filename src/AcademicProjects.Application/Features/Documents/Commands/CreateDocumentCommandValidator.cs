using FluentValidation;

namespace AcademicProjects.Application.Features.Documents.Commands;

public sealed class CreateDocumentCommandValidator
    : AbstractValidator<CreateDocumentCommand>
{
    public const long MaxFileSizeBytes = 50 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".ipynb",
        ".xlsx", ".xls", ".csv",
        ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp",
        ".zip", ".rar", ".7z", ".tar", ".gz",
        ".mp4", ".mov", ".avi", ".webm",
        ".doc", ".docx", ".ppt", ".pptx", ".txt", ".md",
    };

    public CreateDocumentCommandValidator()
    {
        RuleFor(document => document.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(document => document.FileName)
            .Must(fileName => AllowedExtensions.Contains(Path.GetExtension(fileName)))
            .WithMessage("This file type isn't supported.")
            .When(document => !string.IsNullOrWhiteSpace(document.FileName));

        RuleFor(document => document.ContentType)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(document => document.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("The file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"Files must be {MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");

        RuleFor(document => document.ProjectId)
            .NotEmpty();
    }
}
