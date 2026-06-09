namespace avans_1_4_ai_system_integration_api.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}