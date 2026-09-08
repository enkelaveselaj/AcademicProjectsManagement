using AcademicProjects.Application.Features.Documents.Commands;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class UpdateDocumentCommandValidatorTests
{
    private readonly UpdateDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(
            new UpdateDocumentCommand(Guid.NewGuid(), "file.pdf", "/files/file.pdf", Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(
            new UpdateDocumentCommand(Guid.Empty, "file.pdf", "/files/file.pdf", Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateDocumentCommand.Id));
    }
}
