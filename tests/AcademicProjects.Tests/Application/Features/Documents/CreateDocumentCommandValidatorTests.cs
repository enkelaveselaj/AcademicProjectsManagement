using AcademicProjects.Application.Features.Documents.Commands;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class CreateDocumentCommandValidatorTests
{
    private readonly CreateDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "/files/file.pdf", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyFileName_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("", "/files/file.pdf", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FileName));
    }

    [Fact]
    public void Validate_EmptyFilePath_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.FilePath));
    }

    [Fact]
    public void Validate_EmptyProjectId_HasError()
    {
        var result = _validator.Validate(
            new CreateDocumentCommand("file.pdf", "/files/file.pdf", Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateDocumentCommand.ProjectId));
    }
}
