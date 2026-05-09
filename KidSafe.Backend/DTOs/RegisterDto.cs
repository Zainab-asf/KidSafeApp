namespace KidSafe.Backend.DTOs;

public record RegisterDto(string Email, string Password, string DisplayName, string Role);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Role, int UserId, string DisplayName,
                              string? AvatarEmoji = null, string? Email = null);
