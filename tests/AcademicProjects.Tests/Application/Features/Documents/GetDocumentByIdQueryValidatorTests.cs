using AcademicProjects.Application.Features.Documents.Queries;

namespace AcademicProjects.Tests.Application.Features.Documents;

public class GetDocumentByIdQueryValidatorTests
{
    private readonly GetDocumentByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new GetDocumentByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new GetDocumentByIdQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
