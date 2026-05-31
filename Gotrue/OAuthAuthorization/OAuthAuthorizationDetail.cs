using Newtonsoft.Json;

namespace Supabase.Gotrue.OAuthAuthorization
{
	/// <summary>
	/// OAuth authorization details when user needs to provide consent.
	/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
	/// 
	/// This response includes all information needed to display a consent page:
	/// client details, user info, requested scopes, and where the user will be redirected.
	/// 
	/// Note: <see cref="RedirectUri"/> is the base URI (e.g., "https://app.com/callback") without
	/// query parameters. After consent, you'll receive a complete redirect_url with
	/// the authorization code and state parameters appended.
	/// </summary>
	public class OAuthAuthorizationDetail
	{
		/// <summary>
		/// The authorization ID used to approve or deny the request.
		/// </summary>
		[JsonProperty("authorization_id")]
		public string AuthorizationId { get; set; }
		
		/// <summary>
		/// The OAuth client's registered redirect URI (base URI without query parameters).
		/// </summary>
		[JsonProperty("redirect_uri")]
		public string RedirectUri { get; set; }
		
		/// <summary>
		/// OAuth client requesting authorization.
		/// </summary>
		[JsonProperty("client")]
		public OAuthAuthorizationClient Client { get; set; }
		
		/// <summary>
		/// User object associated with the authorization.
		/// </summary>
		[JsonProperty("user")]
		public OAuthAuthorizationUser User { get; set; }
		
		/// <summary>
		/// Space-separated list of requested scopes (e.g., "openid profile email").
		/// </summary>
		[JsonProperty("scope")]
		public string Scope { get; set; }
	}
}