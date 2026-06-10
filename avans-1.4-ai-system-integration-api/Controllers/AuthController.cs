using avans_1._4_ai_system_integration_api.Models;
using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1_4_ai_system_integration_api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace avans_1._4_ai_system_integration_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly TrashDetectionDbContext _context;

        public AuthController(IConfiguration config, TrashDetectionDbContext context)
        {
            _config = config;
            _context = context;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequest dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
            {
                return BadRequest(new { message = "Email already in use" });
            }
            var hasher = new PasswordHasher<string>();

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hasher.HashPassword(null, dto.Password) // hash maken
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Gebruiker aangemaakt" });
        }

        // POST api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Gebruiker niet gevonden" });

            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Wachtwoord onjuist" });

            var token = GenerateToken(user.Email);
            return Ok(new { token });
        }

        private string GenerateToken(string email)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "User")
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
