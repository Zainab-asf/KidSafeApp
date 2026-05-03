namespace KidSafeApp.Backend.Domain.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Parent = "Parent";
    public const string Teacher = "Teacher";
    public const string Child = "Child";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Admin,
        Parent,
        Teacher,
        Child
    };
}
