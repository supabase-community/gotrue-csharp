using Newtonsoft.Json;

namespace Supabase.Gotrue.OAuthAuthorization
{
	/// <summary>
	/// OAuth client details in an authorization request.
	/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
	/// </summary>
	public class OAuthAuthorizationClient
	{
		/// <summary>
		/// Unique identifier for the OAuth client (UUID).
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }
		
		/// <summary>
		/// Human-readable name of the OAuth client.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
		
		/// <summary>
		/// URI of the OAuth client's website.
		/// </summary>
		[JsonProperty("uri")]
		public string Uri { get; set; }
		
		/// <summary>
		/// URI of the OAuth client's logo.
		/// </summary>
		[JsonProperty("logo_uri")]
		public string LogoUri { get; set; }
	}
}