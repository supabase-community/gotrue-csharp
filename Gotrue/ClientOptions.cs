using System.Collections.Generic;
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
        /// Should the Client call <see cref="SessionPersistor"/>, <see cref="SessionRetriever"/>, and <see cref="SessionDestroyer"/>?
        /// </summary>
        public bool PersistSession { get; set; } = true;

        /// <summary>
        /// Function called to persist the session (probably on a filesystem or cookie)
        /// </summary>
        public PersistenceListener.SaveSession? SessionPersistor;

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        public PersistenceListener.LoadSession? SessionRetriever;

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        public PersistenceListener.DestroySession? SessionDestroyer;

        /// <summary>
        /// Very unlikely this flag needs to be changed except in very specific contexts.
        /// 
        /// Enables tests to be E2E tests to be run without requiring users to have
        /// confirmed emails - mirrors the Gotrue server's configuration.
        /// </summary>
        public bool AllowUnconfirmedUserSessions { get; set; }
    }
}
