namespace KidSafe.Backend.Common;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin   = "Admin";
        public const string Teacher = "Teacher";
        public const string Parent  = "Parent";
        public const string Child   = "Child";
    }

    public static class UserStatus
    {
        public const string Active   = "active";
        public const string Pending  = "pending";
        public const string Disabled = "disabled";
    }

    public static class AILabels
    {
        public const string Safe   = "Safe";
        public const string Watch  = "Watch";
        public const string Review = "Review";
    }

    public static class Fcm
    {
        public const string ParentsTopic = "kidsafe-parents";
    }

    public static class Claims
    {
        public const string UserId = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    }
}
