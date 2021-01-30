using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("app_metadata")]
        public object AppMetadata { get; set; }

        [JsonProperty("user_metadata")]
        public object UserMetadata { get; set; }

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
}
