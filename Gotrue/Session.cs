using System;
using Newtonsoft.Json;
#pragma warning disable CS1591

namespace Supabase.Gotrue
{
    /// <summary>
    /// Represents a Gotrue Session
    /// </summary>
    public class Session
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        [JsonProperty("user")]
        public User? User { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The expiration date of this session, in UTC time.
        /// </summary>
        /// <returns></returns>
        public DateTime ExpiresAt() => new DateTimeOffset(CreatedAt).AddSeconds(ExpiresIn).ToUniversalTime().DateTime;
        
        /// <summary>
        /// Returns true if the session has expired
        /// </summary>
        /// <returns></returns>
        public bool Expired() => ExpiresAt() < DateTime.UtcNow;
    }
}
