using Newtonsoft.Json;
using Supabase.Gotrue.Responses.OAuth;

namespace Supabase.Gotrue.OAuth
{
    /// <summary>
    /// Represents the data required to update an OAuth client.
    /// </summary>
    public class UpdateOAuthClient
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

        /// <summary>Token endpoint authentication method.</summary>
        [JsonProperty("token_endpoint_auth_method")]
        public OAuthClientTokenEndpoint? TokenEndpoint { get; set; }

        /// <summary>
        /// Creates an instance of <see cref="UpdateOAuthClient"/> from the given <see cref="OAuthClient"/>.
        /// </summary>
        /// <param name="client">The source <see cref="OAuthClient"/> containing the values to populate the <see cref="UpdateOAuthClient"/>.</param>
        /// <returns>A new instance of <see cref="UpdateOAuthClient"/> populated with the corresponding properties from the <paramref name="client"/>.</returns>
        public static UpdateOAuthClient From(OAuthClient client) =>
            new UpdateOAuthClient
            {
                ClientName = client.ClientName,
                ClientUri = client.ClientUri,
                LogoUri = client.LogoUri,
                RedirectUrls = client.RedirectUris,
                GrantTypes = client.GrantTypes,
                TokenEndpoint = client.TokenEndpointAuthMethod,
            };
    }
}
