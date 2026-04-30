using Newtonsoft.Json;

namespace Supabase.Gotrue.Mfa
{
	public class MfaChallengeAndVerifyParams
	{
		[JsonProperty("factor_id")]
		public string FactorId { get; set; }
		[JsonProperty("code")]
		public string Code { get; set; }
		[JsonProperty("friendly_name")]
		public string? FriendlyName { get; set; }
		[JsonProperty("channel")]
		public string? Channel { get; set; }
		[JsonProperty("webauthn")]
		public WebAuthnChallengeParams? WebAuthn { get; set; }
	}
}