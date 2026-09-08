using AcademicProjects.Application.Features.ProjectAssignments.Commands;

namespace AcademicProjects.Tests.Application.Features.ProjectAssignments;

public class DeleteProjectAssignmentCommandValidatorTests
{
    private readonly DeleteProjectAssignmentCommandValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new DeleteProjectAssignmentCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new DeleteProjectAssignmentCommand(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
