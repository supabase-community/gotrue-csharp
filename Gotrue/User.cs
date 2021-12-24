using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Represents a Gotrue User
    /// </summary>
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("app_metadata")]
        public Dictionary<string, object> AppMetadata { get; set; }

        [JsonProperty("user_metadata")]
        public Dictionary<string, object> UserMetadata { get; set; }

        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("confirmed_at")]
        public DateTime ConfirmedAt { get; set; }

        [JsonProperty("last_sign_in_at")]
        public DateTime LastSignInAt { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }
    }

    public class AdminUserAttributes: UserAttributes
    {
        /// <summary>
        /// A custom data object for user_metadata. Can be any JSON serializable data.
        /// Only a service role can modify.
        /// </summary>
        [JsonProperty("user_metadata")]
        public Dictionary<string, object> UserMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A custom data object for app_metadata that. Can be any JSON serializable data.
        /// Only a service role can modify
        ///
        /// Note: GoTrue does not yest support creating a user with app metadata
        ///     (see: https://github.com/supabase/gotrue-js/blob/d7b334a4283027c65814aa81715ffead262f0bfa/test/GoTrueApi.test.ts#L45)
        /// </summary>
        [JsonProperty("app_metadata")]
        public Dictionary<string, object> AppMetadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Sets if a user has confirmed their email address.
        /// Only a service role can modify
        /// </summary>
        [JsonProperty("email_confirm")]
        public bool EmailConfirm { get; set; }

        /// <summary>
        /// Sets if a user has confirmed their phone number.
        /// Only a service role can modify
        /// </summary>
        [JsonProperty("phone_confirm")]
        public bool PhoneConfirm { get; set; }
    }

    public class UserAttributes
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("email_change_token")]
        public string EmailChangeToken { get; set; }

        [JsonProperty("data")]
        public Dictionary<string, object> Data { get; set; }
    }

    public class VerifyOTPParams
    {
        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public class UserList
    {
        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("users")]
        public List<User> Users { get; set; }
    }
}
