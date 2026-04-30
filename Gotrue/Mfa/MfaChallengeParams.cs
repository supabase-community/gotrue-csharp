using Newtonsoft.Json;

namespace Supabase.Gotrue.Mfa
{
	public class MfaChallengeParams
	{
		// Id of the factor to be challenged. Returned in enroll().
		[JsonProperty("factor_id")]
		public string FactorId { get; set; }

		// Friendly name of the factor, useful for distinguishing between factors
		[JsonProperty("friendly_name")]
		public string? FriendlyName { get; set; }

		// Messaging channel to use (e.g. whatsapp or sms). Only relevant for phone factors
		[JsonProperty("channel")]
		public string? Channel { get; set; }

		// WebAuthn parameters for WebAuthn factor challenge
		[JsonProperty("webauthn")]
		public WebAuthnChallengeParams? WebAuthn { get; set; }
	}

	public class WebAuthnChallengeParams
	{
		// Relying party ID
		[JsonProperty("rp_id")]
		public string? RpId { get; set; }

		// Relying party origins
		[JsonProperty("rp_origins")]
		public string[]? RpOrigins { get; set; }
	}
}