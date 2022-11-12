using Supabase.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Supabase.Gotrue
{
    public static class Constants
    {
        public static string GOTRUE_URL = "http://localhost:9999";
        public static string AUDIENCE = "";
        public static readonly Dictionary<string, string> DEFAULT_HEADERS = new Dictionary<string, string>();
        public static int EXPIRY_MARGIN = 60 * 1000;
        public static string STORAGE_KEY = "supabase.auth.token";
        public static readonly Dictionary<string, object> COOKIE_OPTIONS = new Dictionary<string, object>{
            { "name", "sb:token" },
            { "lifetime", 60 * 60 * 8 },
            { "domain", "" },
            { "path", "/" },
            { "sameSite", "lax" }
        };

        public enum SortOrder
        {
            [MapTo("asc")]
            Ascending,
            [MapTo("desc")]
            Descending
        }

        public enum MobileOtpType
        {
            [MapTo("sms")]
            SMS,
            [MapTo("phone_change")]
            PhoneChange
        }

        public enum EmailOtpType
        {
            [MapTo("signup")]
            Signup,
            [MapTo("invite")]
            Invite,
            [MapTo("magiclink")]
            MagicLink,
            [MapTo("recovery")]
            Recovery,
            [MapTo("email_change")]
            EmailChange
        }

        /// <summary>
        /// Providers available to Supabase
        /// Ref: https://supabase.github.io/gotrue-js/modules.html#Provider
        /// </summary>
        public enum Provider
        {
            [MapTo("apple")]
            Apple,
            [MapTo("azure")]
            Azure,
            [MapTo("bitbucket")]
            Bitbucket,
            [MapTo("discord")]
            Discord,
            [MapTo("facebook")]
            Facebook,
            [MapTo("github")]
            Github,
            [MapTo("gitlab")]
            Gitlab,
            [MapTo("google")]
            Google,
            [MapTo("keycloak")]
            KeyCloak,
            [MapTo("linkedin")]
            LinkedIn,
            [MapTo("notion")]
            Notion,
            [MapTo("slack")]
            Slack,
            [MapTo("spotify")]
            Spotify,
            [MapTo("twitch")]
            Twitch,
            [MapTo("twitter")]
            Twitter,
            [MapTo("workos")]
            WorkOS
        };

        /// <summary>
        /// States that the Auth Client will raise events for.
        /// </summary>
        public enum AuthState
        {
            SignedIn,
            SignedOut,
            UserUpdated,
            PasswordRecovery,
            TokenRefreshed
        };

        /// <summary>
        /// Specifies the functionality expected from the `SignIn` method
        /// </summary>
        public enum SignInType
        {
            Email,
            Phone,
            RefreshToken,
        }

        /// <summary>
        /// Specifies the functionality expected from the `SignUp` method
        /// </summary>
        public enum SignUpType
        {
            Email,
            Phone
        }
    }
}
