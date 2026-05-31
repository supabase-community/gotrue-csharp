using Newtonsoft.Json;

namespace Supabase.Gotrue.OAuthAuthorization
{
	
	/// <summary>
	/// OAuth redirect response when user has already consented or after consent decision.
	/// Only relevant when the OAuth 2.1 server is enabled in Supabase Auth.
	///
	/// This response is returned in three scenarios:
	/// 1. User already consented to these scopes (auto-approved)
	/// 2. User just approved the authorization request
	/// 3. User just denied the authorization request
	///
	/// The `redirect_url` is a complete URL ready for redirecting the user back to the
	/// OAuth client, including authorization code (on success) or error (on denial) in
	/// query parameters, along with the state parameter if one was provided.
	/// </summary>
	public class OAuthAuthorizationRedirect
	{
		/// <summary>
		/// Complete redirect URL with authorization code and state parameters (e.g., "https://app.com/callback?code=xxx&state=yyy")
		/// </summary>
		[JsonProperty("redirect_url")]
		public string RedirectUrl { get; set; }
	}
}