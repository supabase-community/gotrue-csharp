using Newtonsoft.Json;
using System;

namespace Supabase.Gotrue
{
	/// <summary>
	/// Represents an Admin OAuth Client.
	/// </summary>
	public class OAuthClient
	{
		/// <summary>Unique identifier for the OAuth client.</summary>
		[JsonProperty("id")]
		public string? Id { get; set; }

		/// <summary>Display name for the OAuth client.</summary>
		[JsonProperty("name")]
		public string? Name { get; set; }

		/// <summary>Description for the OAuth client.</summary>
		[JsonProperty("description")]
		public string? Description { get; set; }

		/// <summary>Client ID used in OAuth flows.</summary>
		[JsonProperty("client_id")]
		public string? ClientId { get; set; }

		/// <summary>Client Secret used in OAuth flows. Only returned on creation or secret regeneration.</summary>
		[JsonProperty("client_secret")]
		public string? ClientSecret { get; set; }

		/// <summary>Creation timestamp.</summary>
		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		/// <summary>Last updated timestamp.</summary>
		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
