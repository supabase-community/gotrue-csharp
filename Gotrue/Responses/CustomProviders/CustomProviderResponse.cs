using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Supabase.Gotrue.Responses.CustomProviders
{
    /// <summary>
    /// Represents a response containing a list of custom providers.
    /// </summary>
    public class CustomProviderResponse
    {
        /// <summary>
        /// Gets or sets the list of custom providers available in the response.
        /// </summary>
        [JsonProperty("providers")]
        public List<CustomProvider>? Providers { get; set; }
    }
}
