using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Supabase.Gotrue.Constants;

namespace Supabase.Gotrue
{
    /// <summary>
    /// Class represention options available to the <see cref="Client"/>.
    /// </summary>
    public class ClientOptions<TSession>
        where TSession : Session
    {
        /// <summary>
        /// Gotrue Endpoint
        /// </summary>
        public string Url { get; set; } = GOTRUE_URL;

        /// <summary>
        /// Headers to be sent with subsequent requests.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>(DEFAULT_HEADERS);

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
        public Func<TSession, Task<bool>> SessionPersistor = session => Task.FromResult(true);

        /// <summary>
        /// Function to retrieve a session (probably from the filesystem or cookie)
        /// </summary>
        public Func<Task<TSession?>> SessionRetriever = () => Task.FromResult<TSession?>(null);

        /// <summary>
        /// Function to destroy a session.
        /// </summary>
        public Func<Task<bool>> SessionDestroyer = () => Task.FromResult(true);

        /// <summary>
        /// Very unlikely this flag needs to be changed except in very specific contexts.
        /// 
        /// Enables tests to be E2E tests to be run without requiring users to have
        /// confirmed emails - mirrors the Gotrue server's configuration.
        /// </summary>
        public bool AllowUnconfirmedUserSessions { get; set; }
    }
}
