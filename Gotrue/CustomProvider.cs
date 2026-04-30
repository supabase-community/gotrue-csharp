using Newtonsoft.Json;
using System;

namespace Supabase.Gotrue
{
	/// <summary>
	/// Represents an Admin Custom Provider.
	/// </summary>
	public class CustomProvider
	{
		/// <summary>Unique identifier for the custom provider.</summary>
		[JsonProperty("id")]
		public string? Id { get; set; }

		/// <summary>Display name for the custom provider.</summary>
		[JsonProperty("name")]
		public string? Name { get; set; }

		/// <summary>The type of provider (e.g. oidc, saml).</summary>
		[JsonProperty("type")]
		public string? Type { get; set; }

		/// <summary>Creation timestamp.</summary>
		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		/// <summary>Last updated timestamp.</summary>
		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
