namespace AcademicProjects.API.Authentication;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
