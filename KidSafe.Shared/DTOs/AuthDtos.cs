namespace KidSafe.Shared.DTOs;

public record RegisterRequest(string Email, string Password, string DisplayName, string Role);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Role, int UserId, string DisplayName);
