namespace avans_1._4_ai_system_integration_api.DTOs
{
    public class RegisterDTO
    {
        public record RegisterRequest(string Email, string Password);
    }
}
