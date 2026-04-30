using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue.Responses
{
	public class CustomProviderResponse
	{
	/// <summary>Unique identifier (UUID).</summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>Provider type.</summary>
    [JsonProperty("provider_type")]
    public string? ProviderType { get; set; }

    /// <summary>Provider identifier, e.g. <c>custom:mycompany</c>.</summary>
    [JsonProperty("identifier")]
    public string? Identifier { get; set; }

    /// <summary>Human-readable name.</summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>OAuth client ID.</summary>
    [JsonProperty("client_id")]
    public string? ClientId { get; set; }

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

    /// <summary>OIDC discovery document (OIDC providers only).</summary>
    [JsonProperty("discovery_document")]
    public OIDCDiscoveryDocumentResponse? DiscoveryDocument { get; set; }

    /// <summary>Timestamp when the provider was created.</summary>
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>Timestamp when the provider was last updated.</summary>
    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }	
	}
}