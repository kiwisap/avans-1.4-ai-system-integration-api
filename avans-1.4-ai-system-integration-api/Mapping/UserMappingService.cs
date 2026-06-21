using avans_1._4_ai_system_integration_api.Models.Entities;
using avans_1._4_ai_system_integration_api.Mapping.Interfaces;
using avans_1._4_ai_system_integration_api.Models.DTOs;

namespace avans_1._4_ai_system_integration_api.Mapping;

public class UserMappingService : IUserMappingService
{
    public User RegisterDtoToUser(RegisterDTO registerDto)
    {
        return new User
        {
            Name = registerDto.Name,
            UserName = registerDto.Email,
            Email = registerDto.Email
        };
    }

    public UserDTO UserToUserDto(User user)
    {
        return new UserDTO
        {
            Name = user.Name,
            Email = user.Email,
        };
    }
}