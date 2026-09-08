using AcademicProjects.Application.Features.Documents.Commands;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class DeleteDocumentCommandValidatorTests
{
    private readonly DeleteDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new DeleteDocumentCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteDocumentCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
