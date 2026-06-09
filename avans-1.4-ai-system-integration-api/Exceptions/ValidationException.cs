namespace avans_1_4_ai_system_integration_api.Exceptions;

public sealed class ValidationException : AppException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}