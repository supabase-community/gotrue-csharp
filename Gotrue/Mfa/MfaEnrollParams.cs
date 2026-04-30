using Newtonsoft.Json;

namespace Supabase.Gotrue.Mfa
{
	public class MfaEnrollParams
	{
		[JsonProperty("factor_type")]
		public string FactorType { get; set; }

		[JsonProperty("issuer")]
		public string? Issuer { get; set; }

		[JsonProperty("friendly_name")]
		public string? FriendlyName { get; set; }
	}
}