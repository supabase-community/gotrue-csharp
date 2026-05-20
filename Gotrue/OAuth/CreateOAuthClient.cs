using Newtonsoft.Json;
using Supabase.Gotrue.Responses.OAuth;

namespace Supabase.Gotrue.OAuth
{
    /// <summary>
    /// Represents the data required to create a new OAuth client.
    /// </summary>
    public class CreateOAuthClient
    {
        /// <summary>Human-readable name of the OAuth client.</summary>
        [JsonProperty("client_name")]
        public string? ClientName { get; set; }

        /// <summary>URI of the OAuth client.</summary>
        [JsonProperty("client_uri")]
        public string? ClientUri { get; set; }

        /// <summary>URI of the OAuth client's logo.</summary>
        [JsonProperty("logo_uri")]
        public string? LogoUri { get; set; }

        /// <summary>Array of allowed redirect URIs.</summary>
        [JsonProperty("redirect_uris")]
        public string[]? RedirectUrls { get; set; }

        /// <summary>Array of allowed grant types.</summary>
        [JsonProperty("grant_types")]
        public OAuthClientGrantType[]? GrantTypes { get; set; }

        /// <summary>Array of allowed response types.</summary>
        [JsonProperty("response_types")]
        public string? ResponsType { get; set; }

        /// <summary>Scope of the OAuth client.</summary>
        [JsonProperty("scope")]
        public string? Scope { get; set; }

        /// <summary>Token endpoint authentication method.</summary>
        [JsonProperty("token_endpoint_auth_method")]
        public OAuthClientTokenEndpoint? TokenEndpoint { get; set; }
    }
}
