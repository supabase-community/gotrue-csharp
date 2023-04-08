using System;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Represents an OAuth Provider's URI and Parameters.
    /// </summary>
    public class ProviderUri
    {
        /// <summary>
        /// The Generated Provider's URI
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The PKCE Verifier, only set during a PKCE auth flow.
        /// </summary>
        public string? PKCEVerifier { get; set; }

        public ProviderUri(Uri uri)
        {
            Uri = uri;
        }
    }
}
