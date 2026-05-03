namespace KidSafeApp.Constants;

/// <summary>
/// Centralized API endpoint constants.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Base API path.
    /// </summary>
    public const string BasePath = "api";

    /// <summary>
    /// Authentication endpoints.
    /// </summary>
    public static class Auth
    {
        public const string Login = $"{BasePath}/auth/login";
        public const string Register = $"{BasePath}/auth/register";
        public const string Logout = $"{BasePath}/auth/logout";
    }

    /// <summary>
    /// Messages/Chat endpoints.
    /// </summary>
    public static class Messages
    {
        public const string Base = $"{BasePath}/messages";
        public const string Send = Base;
        public const string GetHistory = $"{Base}/{{userId}}";
        public const string MarkAsRead = $"{Base}/{{messageId}}/read";
        public const string ChatPreviews = $"{Base}/chats/preview";
    }

    /// <summary>
    /// Admin endpoints.
    /// </summary>
    public static class Admin
    {
        public const string Base = $"{BasePath}/admin";
        public const string Users = $"{Base}/users";
        public const string UsersPaged = $"{Users}/paged";
        public const string ClassRooms = $"{Base}/classrooms";
        public const string Courses = $"{Base}/courses";
    }

    /// <summary>
    /// User management endpoints.
    /// </summary>
    public static class Users
    {
        public const string Base = $"{BasePath}/users";
        public const string GetUsers = Base;
        public const string GetChats = $"{Base}/chats";
    }
}