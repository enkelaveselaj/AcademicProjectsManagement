namespace AcademicProjects.Application.Common.Exceptions;

public sealed class InvalidCredentialsException(string message) : Exception(message);
