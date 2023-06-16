﻿using System;
using Newtonsoft.Json;

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
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt() => new DateTime(CreatedAt.Ticks).AddSeconds(ExpiresIn);
        
        /// <summary>
        /// Returns true if the session has expired
        /// </summary>
        /// <returns></returns>
        public bool Expired() => ExpiresAt() < DateTime.Now;
    }
}
