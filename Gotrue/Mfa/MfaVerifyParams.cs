using Newtonsoft.Json;

namespace Supabase.Gotrue.Mfa
{
	public class MfaVerifyParams
	{
		// ID of the factor being verified. Returned in enroll()
		[JsonProperty("factor_id")]
		public string FactorId { get; set; }

		// ID of the challenge being verified. Returned in challenge()
		[JsonProperty("challenge_id")]
		public string ChallengeId { get; set; }

		// Verification code provided by the user
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// For WebAuthn verification
		/// </summary>
		[JsonProperty("webauthn")]
		public object? WebAuthn { get; set; }
	}
}