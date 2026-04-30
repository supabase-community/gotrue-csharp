using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Represents an Admin Custom Provider.
    /// </summary>
    public class CustomProvider
    {
        /// <summary>Provider type.</summary>
        [JsonProperty("provider_type")]
        public CustomProviderType? ProviderType { get; set; }

        /// <summary>Provider identifier, e.g. <c>custom:mycompany</c>.</summary>
        [JsonProperty("identifier")]
        public string? Identifier { get; set; }

        /// <summary>Human-readable name.</summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>OAuth client ID.</summary>
        [JsonProperty("client_id")]
        public string? ClientId { get; set; }

        /// <summary>OAuth client secret (write-only, not returned in responses).</summary>
        [JsonProperty("client_secret")]
        public string? ClientSecret { get; set; }

        /// <summary>Additional client IDs accepted during token validation.</summary>
        [JsonProperty("acceptable_client_ids")]
        public List<string>? AcceptableClientIds { get; set; }

        /// <summary>OAuth scopes requested during authorization.</summary>
        [JsonProperty("scopes")]
        public List<string>? Scopes { get; set; }

        /// <summary>Whether PKCE is enabled.</summary>
        [JsonProperty("pkce_enabled")]
        public bool? PkceEnabled { get; set; }

        /// <summary>Mapping of provider attributes to Supabase user attributes.</summary>
        [JsonProperty("attribute_mapping")]
        public Dictionary<string, object>? AttributeMapping { get; set; }

        /// <summary>Additional parameters sent with the authorization request.</summary>
        [JsonProperty("authorization_params")]
        public Dictionary<string, string>? AuthorizationParams { get; set; }

        /// <summary>Whether the provider is enabled.</summary>
        [JsonProperty("enabled")]
        public bool? Enabled { get; set; }

        /// <summary>Whether email is optional for this provider.</summary>
        [JsonProperty("email_optional")]
        public bool? EmailOptional { get; set; }

        /// <summary>OIDC issuer URL.</summary>
        [JsonProperty("issuer")]
        public string? Issuer { get; set; }

        /// <summary>OIDC discovery URL.</summary>
        [JsonProperty("discovery_url")]
        public string? DiscoveryUrl { get; set; }

        /// <summary>Whether to skip nonce check (OIDC).</summary>
        [JsonProperty("skip_nonce_check")]
        public bool? SkipNonceCheck { get; set; }

        /// <summary>OAuth2 authorization URL.</summary>
        [JsonProperty("authorization_url")]
        public string? AuthorizationUrl { get; set; }

        /// <summary>OAuth2 token URL.</summary>
        [JsonProperty("token_url")]
        public string? TokenUrl { get; set; }

        /// <summary>OAuth2 userinfo URL.</summary>
        [JsonProperty("userinfo_url")]
        public string? UserinfoUrl { get; set; }

        /// <summary>JWKS URI for token verification.</summary>
        [JsonProperty("jwks_uri")]
        public string? JwksUri { get; set; }
    }
}
