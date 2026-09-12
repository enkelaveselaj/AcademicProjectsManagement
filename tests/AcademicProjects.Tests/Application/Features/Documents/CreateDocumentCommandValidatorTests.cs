using AcademicProjects.Application.Features.Documents.Commands;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class CreateDocumentCommandValidatorTests
{
    private readonly CreateDocumentCommandValidator _validator = new();

    private static MemoryStream SampleContent() => new([1, 2, 3, 4]);

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "application/pdf", 4, SampleContent(), Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyFileName_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("", "application/pdf", 4, SampleContent(), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FileName));
    }

    [Fact]
    public void Validate_UnsupportedExtension_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("malware.exe", "application/octet-stream", 4, SampleContent(), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FileName));
    }

    [Fact]
    public void Validate_EmptyFile_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "application/pdf", 0, SampleContent(), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FileSizeBytes));
    }

    [Fact]
    public void Validate_FileTooLarge_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand(
                "file.pdf", "application/pdf", CreateDocumentCommandValidator.MaxFileSizeBytes + 1, SampleContent(), Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FileSizeBytes));
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "application/pdf", 4, SampleContent(), Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.ProjectId));
    }
}
