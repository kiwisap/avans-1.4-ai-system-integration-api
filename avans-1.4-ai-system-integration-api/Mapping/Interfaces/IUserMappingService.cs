using avans_1._4_ai_system_integration_api.Models.DTOs;
using avans_1._4_ai_system_integration_api.Models.Entities;

namespace avans_1._4_ai_system_integration_api.Mapping.Interfaces;
    public interface IUserMappingService
    {
        User RegisterDtoToUser(RegisterDTO registerDto);
        UserDTO UserToUserDto(User user);
    }

