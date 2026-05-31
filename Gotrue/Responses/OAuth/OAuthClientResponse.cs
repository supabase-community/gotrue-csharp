using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue.Responses.OAuth
{
    /// <summary>
    /// Represents a response containing a list of OAuth clients.
    /// </summary>
    public class OAuthClientResponse
    {
        /// <summary>Represents a collection of OAuth clients.</summary>
        [JsonProperty("clients")]
        public List<OAuthClient> Clients { get; set; } = new List<OAuthClient>();
    }
}
