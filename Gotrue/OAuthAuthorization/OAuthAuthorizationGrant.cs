using Newtonsoft.Json;

namespace Supabase.Gotrue.OAuthAuthorization
{
	/// <summary>
	/// An OAuth grant representing a user's authorization of an OAuth client.
	/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
	/// </summary>
	public class OAuthAuthorizationGrant
	{
		/// <summary>
		/// OAuth client information
		/// </summary>
		[JsonProperty("client")]
		public OAuthAuthorizationClient Client { get; set; }
		
		/// <summary>
		/// Array of scopes granted to this client
		/// </summary>
		[JsonProperty("scopes")]
		public string[] Scopes { get; set; }
		
		/// <summary>
		/// Timestamp when the grant was created (ISO 8601 date-time)
		/// </summary>
		[JsonProperty("granted_at")]
		public string GrantedAt { get; set; }
	}
}