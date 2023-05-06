using System.Collections.Generic;
using Supabase.Gotrue.Interfaces;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Class representation options available to the <see cref="Client"/>.
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Gotrue Endpoint
        /// </summary>
        public string Url { get; set; } = GOTRUE_URL;

        /// <summary>
        /// Headers to be sent with subsequent requests.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// Should the Client automatically handle refreshing the User's Token?
        /// </summary>
        public bool AutoRefreshToken { get; set; } = true;

        /// <summary>
        /// Very unlikely this flag needs to be changed except in very specific contexts.
        /// 
        /// Enables tests to be E2E tests to be run without requiring users to have
        /// confirmed emails - mirrors the Gotrue server's configuration.
        /// </summary>
        public bool AllowUnconfirmedUserSessions { get; set; }
    }
}
