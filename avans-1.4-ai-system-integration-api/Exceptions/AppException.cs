namespace avans_1_4_ai_system_integration_api.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}