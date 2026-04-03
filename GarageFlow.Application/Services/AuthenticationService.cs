using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using Serilog;

namespace GarageFlow.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger _logger;

    public AuthenticationService(IRepository<User> repo, IPasswordHasher hasher, ILogger logger) { _repo = repo; _hasher = hasher; _logger = logger; }

    public async Task<UserDto?> LoginAsync(string username, string password)
    {
        var users = await _repo.FindAsync(u => u.Username == username);
        var user = users.FirstOrDefault();
        if (user is null || !_hasher.Verify(password, user.PasswordHash))
        {
            _logger.Warning("Mislukte inlogpoging voor: {Username}", username);
            return null;
        }
        _logger.Information("Gebruiker ingelogd: {Username}", username);
        return new UserDto { Id = user.Id, Username = user.Username, FullName = user.FullName, Role = user.Role };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User { Username = dto.Username, PasswordHash = _hasher.Hash(dto.Password), FullName = dto.FullName, Role = dto.Role };
        var created = await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();
        return new UserDto { Id = created.Id, Username = created.Username, FullName = created.FullName, Role = created.Role };
    }

    public async Task ChangePasswordAsync(int userId, string newPassword)
    {
        var user = await _repo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("Gebruiker niet gevonden.");
        user.PasswordHash = _hasher.Hash(newPassword);
        await _repo.UpdateAsync(user);
        await _repo.SaveChangesAsync();
    }
}
