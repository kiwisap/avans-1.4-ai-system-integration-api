namespace avans_1_4_ai_system_integration_api.Exceptions;

public sealed class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message) { }
}