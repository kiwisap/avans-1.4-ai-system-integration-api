namespace avans_1_4_ai_system_integration_api.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
}