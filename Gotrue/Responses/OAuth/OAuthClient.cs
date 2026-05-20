using System;
using Newtonsoft.Json;
using Supabase.Gotrue.OAuth;

namespace Supabase.Gotrue.Responses.OAuth
{
    /// <summary>
    /// Represents an Admin OAuth Client.
    /// </summary>
    public class OAuthClient
    {
        /// <summary>Unique identifier for the OAuth client.</summary>
        [JsonProperty("client_id")]
        public string? ClientId { get; set; }

        /// <summary>Human-readable name of the OAuth client.</summary>
        [JsonProperty("client_name")]
        public string? ClientName { get; set; }

        /// <summary>Client secret (only returned on registration and regeneration).</summary>
        [JsonProperty("client_secret")]
        public string? ClientSecret { get; set; }

        /// <summary>Type of OAuth client.</summary>
        [JsonProperty("client_type")]
        public OAuthClientType? ClientType { get; set; }

        /// <summary>Token endpoint authentication method.</summary>
        [JsonProperty("token_endpoint_auth_method")]
        public OAuthClientTokenEndpoint? TokenEndpointAuthMethod { get; set; }

        /// <summary>Registration type of the client.</summary>
        [JsonProperty("registration_type")]
        public OAuthClientRegistrationType? RegistrationType { get; set; }

        /// <summary>URI of the OAuth client.</summary>
        [JsonProperty("client_uri")]
        public string? ClientUri { get; set; }

        /// <summary>URI of the OAuth client's logo.</summary>
        [JsonProperty("logo_uri")]
        public string? LogoUri { get; set; }

        /// <summary>Array of allowed redirect URIs.</summary>
        [JsonProperty("redirect_uris")]
        public string[]? RedirectUris { get; set; }

        /// <summary>Array of allowed grant types.</summary>
        [JsonProperty("grant_types")]
        public OAuthClientGrantType[]? GrantTypes { get; set; }

        /// <summary>Array of allowed response types.</summary>
        [JsonProperty("response_types")]
        public OAuthClientResponseType[]? ResponseTypes { get; set; }

        /// <summary>Scope of the OAuth client.</summary>
        [JsonProperty("scope")]
        public string? Scope { get; set; }

        /// <summary>Timestamp when the client was created.</summary>
        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>Timestamp when the client was last updated.</summary>
        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
