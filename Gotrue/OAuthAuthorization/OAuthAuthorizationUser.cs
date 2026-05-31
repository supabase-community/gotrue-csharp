using Newtonsoft.Json;

namespace Supabase.Gotrue.OAuthAuthorization
{
	/// <summary>
	/// User object associated with the authorization
	/// </summary>
	public class OAuthAuthorizationUser
	{
		/// <summary>
		/// User ID (UUID)
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }
		
		/// <summary>
		/// User email
		/// </summary>
		[JsonProperty("email")]
		public string Email { get; set; }
	}
}