using AcademicProjects.Application.Features.Notifications.Queries;

namespace AcademicProjects.Tests.Application.Features.Notifications;

public class GetNotificationByIdQueryValidatorTests
{
    private readonly GetNotificationByIdQueryValidator _validator = new();

    [Fact]
    public void Validate_NonEmptyId_IsValid()
    {
        var result = _validator.Validate(new GetNotificationByIdQuery(Guid.NewGuid()));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyId_HasError()
    {
        var result = _validator.Validate(new GetNotificationByIdQuery(Guid.Empty));

        Assert.False(result.IsValid);
    }
}
