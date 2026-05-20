using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue.Responses.CustomProviders
{
    /// <summary>
    /// Represents the discovery document response obtained from an OpenID Connect (OIDC) provider.
    /// This class is used to map configuration details of the OIDC provider that
    /// are required for authentication and authorization workflows.
    /// </summary>
    public class OIDCDiscoveryDocumentResponse
    {
        /// <summary>The issuer identifier.</summary>
        [JsonProperty("issuer")]
        public string? Issuer { get; set; }

        /// <summary>URL of the authorization endpoint.</summary>
        [JsonProperty("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }

        /// <summary>URL of the token endpoint.</summary>
        [JsonProperty("token_endpoint")]
        public string? TokenEndpoint { get; set; }

        /// <summary>URL of the JSON Web Key Set.</summary>
        [JsonProperty("jwks_uri")]
        public string? JwksUri { get; set; }

        /// <summary>URL of the userinfo endpoint.</summary>
        [JsonProperty("userinfo_endpoint")]
        public string? UserinfoEndpoint { get; set; }

        /// <summary>URL of the revocation endpoint.</summary>
        [JsonProperty("revocation_endpoint")]
        public string? RevocationEndpoint { get; set; }

        /// <summary>List of supported scopes.</summary>
        [JsonProperty("supported_scopes")]
        public List<string>? SupportedScopes { get; set; }

        /// <summary>List of supported response types.</summary>
        [JsonProperty("supported_response_types")]
        public List<string>? SupportedResponseTypes { get; set; }

        /// <summary>List of supported subject types.</summary>
        [JsonProperty("supported_subject_types")]
        public List<string>? SupportedSubjectTypes { get; set; }

        /// <summary>List of supported ID token signing algorithms.</summary>
        [JsonProperty("supported_id_token_signing_algs")]
        public List<string>? SupportedIdTokenSigningAlgs { get; set; }
    }
}
