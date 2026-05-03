using KidSafeApp.Shared.DTOs.Auth;

namespace KidSafeApp.Services.Interfaces;

/// <summary>
/// Provides authentication operations and state management.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Gets a value indicating whether the user is currently authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the current authenticated user, or null if not authenticated.
    /// </summary>
    UserDto? CurrentUser { get; }

    /// <summary>
    /// Gets the current authentication token, or null if not authenticated.
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// Validates the current authentication token.
    /// </summary>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    Task<bool> IsTokenValidAsync();

    /// <summary>
    /// Logs out the current user and clears all authentication state.
    /// </summary>
    Task LogoutAsync();
}