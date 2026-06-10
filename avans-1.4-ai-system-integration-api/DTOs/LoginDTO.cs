namespace avans_1._4_ai_system_integration_api.DTOs
{
    public class LoginDTO
    {
        public record LoginRequest(string Email, string Password);
    }
}
