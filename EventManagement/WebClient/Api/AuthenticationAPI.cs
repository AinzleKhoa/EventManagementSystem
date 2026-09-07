namespace WebClient.Api
{
    public static class AuthenticationAPI
    {
        public static class UsersOData
        {
            public const string Base = "authentication-gateway/UsersOData";
        }
        //user
        public static class User
        {
            public const string Base = "authentication-gateway/users";
            public const string ById = "authentication-gateway/users/{id}";
            public const string LockAndUnlock = "authentication-gateway/users/lock-unlock/{id}";
        }
        //auth
        public const string Login = "authentication-gateway/auth/login";
        public const string Register = "authentication-gateway/auth/register";
        public const string GoogleLogin = "authentication-gateway/auth/google-login";
        public const string ForgotPassword = "authentication-gateway/auth/forgot-password";
        public const string ResetPassword = "authentication-gateway/auth/reset-password";
        public const string RefreshToken = "authentication-gateway/auth/refresh-token";
        //account
        public static class Profile
        {
            public const string Base = "authentication-gateway/profile";
            public const string ChangePassword = "authentication-gateway/profile/change-password";
            public const string Avatar = "authentication-gateway/profile/avatar";
            public const string IsGoogleAccount = "authentication-gateway/profile/is-google-account";
            public const string Logout = "authentication-gateway/profile/logout";
            public const string RequestBecomeOrganizer = "authentication-gateway/profile/become-organizer/request";
            public const string VerifyBecomeOrganizer = "authentication-gateway/profile/become-organizer/verify";
        }
    }
}
