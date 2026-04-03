using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IAuthenticationService
{
    Task<UserDto?> LoginAsync(string username, string password);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task ChangePasswordAsync(int userId, string newPassword);
}
